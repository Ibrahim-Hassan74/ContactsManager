﻿using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Sevices.Helpers;
using System.Reflection;
using ServiceContracts.Enums;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using OfficeOpenXml;
using RepositoryContracts;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Serilog;
using SerilogTimings;
using Excetions;

namespace Sevices
{
    public class PersonsGetterService : IPersonsGetterService
    {
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsGetterService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;
        //private readonly ICountriesService _countries;
        public PersonsGetterService(IPersonsRepository personsRepository, ILogger<PersonsGetterService> logger, IDiagnosticContext diagnosticContext)
        {
            //_countries = countriesService;
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public virtual async Task<List<PersonResponse>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons of PersonsService");
            var persons = await _personsRepository.GetAllPersons();
            return persons.Select(x => x.ToPersonResponse()).ToList();
            //return _db.sp_GetAllPersons().Select(x => x.ToPersonResponse()).ToList();
        }

        public virtual async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
        {
            if (personID is null)
                return null;
            //Person? person = await _personsRepository.Persons.Include(p => p.Country).FirstOrDefaultAsync(x => x.PersonID == personID);
            Person? person = await _personsRepository.GetPersonByID(personID.Value);
            if (person is null)
                return null;
            return person.ToPersonResponse();
        }

        #region GetFilteredPersons after get all of them
        //public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        //{
        //    List<PersonResponse> personResponses = await GetAllPersons();
        //    List<PersonResponse> matchingPersons = personResponses;

        //    if (string.IsNullOrEmpty(searchString) || string.IsNullOrEmpty(searchBy))
        //        return matchingPersons;


        //    #region using conditional statament switch 
        //    //switch (searchBy)
        //    //{
        //    //    case nameof(Person.PersonName):
        //    //        matchingPersons = personResponses.Where(x =>
        //    //        (string.IsNullOrEmpty(x.PersonName) ? true :
        //    //        x.PersonName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        //    //        ).ToList();
        //    //        break;

        //    //    case nameof(Person.Email):
        //    //        matchingPersons = personResponses.Where(x =>
        //    //        (string.IsNullOrEmpty(x.Email) ? true :
        //    //        x.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        //    //        ).ToList();
        //    //        break;

        //    //    case nameof(Person.DateOfBirth):
        //    //        matchingPersons = personResponses.Where(x =>
        //    //        (x.DateOfBirth is null ? true :
        //    //        x.DateOfBirth.Value.ToString("dd MMMM yyyy")
        //    //        .Contains(searchString, StringComparison.OrdinalIgnoreCase))
        //    //        ).ToList();
        //    //        break;

        //    //    case nameof(Person.Gender):
        //    //        matchingPersons = personResponses.Where(x =>
        //    //        (string.IsNullOrEmpty(x.Gender) ? true :
        //    //        x.Gender.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        //    //        ).ToList();
        //    //        break;

        //    //    case nameof(Person.CountryID):
        //    //        matchingPersons = personResponses.Where(x =>
        //    //        (string.IsNullOrEmpty(x.Country) ? true :
        //    //        x.Country.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        //    //        ).ToList();
        //    //        break;

        //    //    case nameof(Person.Address):
        //    //        matchingPersons = personResponses.Where(x =>
        //    //        (string.IsNullOrEmpty(x.Address) ? true :
        //    //        x.Address.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        //    //        ).ToList();
        //    //        break;
        //    //    default:
        //    //        matchingPersons = personResponses;
        //    //        break;
        //    //}
        //    #endregion

        //    #region using reflection 

        //    PropertyInfo? propertyInfo = typeof(PersonResponse).GetProperty(searchBy,
        //        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        //    if (propertyInfo is null)
        //        return matchingPersons;

        //    matchingPersons = personResponses.Where(x =>
        //    {
        //        var propertyValue = propertyInfo.GetValue(x)?.ToString();
        //        return string.IsNullOrEmpty(propertyValue) || propertyValue.Contains(searchString, StringComparison.OrdinalIgnoreCase);
        //    }).ToList();

        //    #endregion

        //    return matchingPersons;
        //}
        #endregion
        public virtual async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            _logger.LogInformation("GetFilteredPersons in PersonsService");
            if(string.IsNullOrEmpty(searchString) || string.IsNullOrEmpty(searchBy))
            {
                return await GetAllPersons();
            }

            List<Person> persons;
            using (Operation.Time("Time for filtered persons from database"))
            {
                persons = searchBy switch
                {
                    nameof(PersonResponse.PersonName) =>
                     await _personsRepository.GetFilteredPersons(temp =>
                     temp.PersonName.Contains(searchString)),

                    nameof(PersonResponse.Email) =>
                     await _personsRepository.GetFilteredPersons(temp =>
                     temp.Email.Contains(searchString)),

                    nameof(PersonResponse.DateOfBirth) =>
                    (DateTime.TryParseExact(searchString, "dd MMMM yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                    ? await _personsRepository.GetFilteredPersons(temp =>
                        temp.DateOfBirth.HasValue &&
                        temp.DateOfBirth.Value.Year == parsedDate.Year &&
                        temp.DateOfBirth.Value.Month == parsedDate.Month &&
                        temp.DateOfBirth.Value.Day == parsedDate.Day)
                    : await _personsRepository.GetAllPersons(),


                    nameof(PersonResponse.Gender) =>
                     await _personsRepository.GetFilteredPersons(temp =>
                     temp.Gender.Contains(searchString)),

                    nameof(PersonResponse.CountryID) =>
                     await _personsRepository.GetFilteredPersons(temp =>
                     temp.Country.CountryName.Contains(searchString)),

                    nameof(PersonResponse.Address) =>
                    await _personsRepository.GetFilteredPersons(temp =>
                    temp.Address.Contains(searchString)),

                    nameof(PersonResponse.ReciveNewsLetters) =>
                    await _personsRepository
                    .GetFilteredPersons(temp => temp.ReciveNewsLetters.ToString().Contains(searchString)),

                    _ => await _personsRepository.GetAllPersons()
                };
            }
            _diagnosticContext.Set("Persons", persons);

            return persons.Select(temp => temp.ToPersonResponse()).ToList();
        }

        public virtual async Task<MemoryStream> GetPersonsCSV()
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);

            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);

            CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration);

            //csvWriter.WriteHeader<PersonResponse>(); // PersonID, PersonName, Email ....
            csvWriter.WriteField(nameof(PersonResponse.PersonName));
            csvWriter.WriteField(nameof(PersonResponse.Email));
            csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
            csvWriter.WriteField(nameof(PersonResponse.Age));
            //csvWriter.WriteField(nameof(PersonResponse.Gender));
            csvWriter.WriteField(nameof(PersonResponse.Country));
            csvWriter.WriteField(nameof(PersonResponse.Address));
            csvWriter.WriteField(nameof(PersonResponse.ReciveNewsLetters));
            csvWriter.NextRecord();

            List<PersonResponse> persons = await GetAllPersons();

            foreach (var person in persons)
            {
                csvWriter.WriteField(person.PersonName);
                csvWriter.WriteField(person.Email);
                if (person.DateOfBirth is not null)
                    csvWriter.WriteField(person.DateOfBirth.Value.ToString("yyyy-MM-dd"));
                else
                    csvWriter.WriteField(string.Empty);
                csvWriter.WriteField(person.Age);
                //csvWriter.WriteField(person.Gender);
                csvWriter.WriteField(person.Country);
                csvWriter.WriteField(person.Address);
                csvWriter.WriteField(person.ReciveNewsLetters);
                csvWriter.NextRecord();
                csvWriter.Flush();
            }

            //await csvWriter.WriteRecordsAsync(persons);

            memoryStream.Position = 0;
            return memoryStream;
        }

        public virtual async Task<MemoryStream> GetPersonsExcel()
        {
            MemoryStream memoryStream = new MemoryStream();

            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");

                excelWorksheet.Cells["A1"].Value = "Person Name";
                excelWorksheet.Cells["B1"].Value = "Email";
                excelWorksheet.Cells["C1"].Value = "Date of Birth";
                excelWorksheet.Cells["D1"].Value = "Age";
                excelWorksheet.Cells["E1"].Value = "Gender";
                excelWorksheet.Cells["F1"].Value = "Country";
                excelWorksheet.Cells["G1"].Value = "Address";
                excelWorksheet.Cells["H1"].Value = "Receive News Letters";

                using (ExcelRange headerCells = excelWorksheet.Cells["A1:H1"])
                {
                    headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    headerCells.Style.Font.Bold = true;
                }

                int row = 2;
                List<PersonResponse> persons = await GetAllPersons();

                foreach (var person in persons)
                {
                    excelWorksheet.Cells[row, 1].Value = person.PersonName;
                    excelWorksheet.Cells[row, 2].Value = person.Email;
                    if (person.DateOfBirth.HasValue)
                        excelWorksheet.Cells[row, 3].Value = person.DateOfBirth.Value.ToString("yyyy-MM-dd");
                    excelWorksheet.Cells[row, 4].Value = person.Age;
                    excelWorksheet.Cells[row, 5].Value = person.Gender;
                    excelWorksheet.Cells[row, 6].Value = person.Country;
                    excelWorksheet.Cells[row, 7].Value = person.Address;
                    excelWorksheet.Cells[row++, 8].Value = person.ReciveNewsLetters;

                }

                excelWorksheet.Cells[$"A1:H{row}"].AutoFitColumns();
                await excelPackage.SaveAsync();

            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
