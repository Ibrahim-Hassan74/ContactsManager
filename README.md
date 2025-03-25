# Contacts Managers

A .NET-based Contacts Manager application that allows users to manage their contacts efficiently. This project follows a structured architecture with separate layers for Core, Infrastructure, UI, and Testing.

## Features

- **CRUD Operations**: Add, update, delete, and view contacts.
- **MVC Architecture**: Separation of concerns for better maintainability.
- **Entity Framework Core**: Database interactions using EF Core.
- **Identity and Authentication**: Secure user authentication and authorization.
- **User Roles**: Admin and User roles with different access permissions.
- **Register/Login/Logout**: User authentication with ASP.NET Identity.
- **Unit and Integration Testing**: Ensures application reliability.
- **Responsive UI**: User-friendly interface built with ASP.NET MVC.

## Project Structure

```
ContactsManagerSolution/
│── ContactsManager.UI/             # User interface (MVC project)
│── ContactsManager.Core/           # Domain models and business logic
│── ContactsManager.Infrastructure/ # Database access and services
│── ContactsManager.ControllerTests/  # Tests for controllers
│── ContactsManager.ServiceTests/     # Tests for services
│── ContactsManager.IntegrationTests/ # Integration tests
│── ContactsManagerSolution.sln       # Solution file
│── .gitignore
```

## Getting Started

### Prerequisites

- .NET 6.0 or later
- SQL Server
- Visual Studio (Recommended) or any compatible IDE

### Installation

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/Ibrahim-Hassan74/ContactsManager.git
   cd ContactsManagerSolution
   ```

2. **Set Up the Database:**

   - Update the connection string in `appsettings.json`.
   - Run migrations:
     ```bash
     dotnet ef database update
     ```

3. **Run the Application:**

   ```bash
   dotnet run --project ContactsManager.UI
   ```

4. Open `http://localhost:5000` in your browser.

## Authentication & Authorization

- **User Registration & Login**: Users can create an account and log in.
- **Roles Management**: The system has Admin and User roles.
- **Admin Privileges**: Admins can manage users and contacts.
- **Secure Access**: Protected routes based on user roles.

## Live Demo

Check out the project live at: [Contacts Managers Live](http://contactsmanager.runasp.net/)

## Testing

Run the tests using:

```bash
dotnet test
```

## Contributing

Feel free to submit issues and pull requests to improve the project.
