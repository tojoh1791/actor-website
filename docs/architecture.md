# Architecture

## Frontend

React-based frontend application using TypeScript

The excact React framework and build setup has not yet been decided. Possible options include a standard React setup or Next.js

The frontend contains:

- Public website
- Administration interface

Responsibilities: 
- User interface
- Navigation
- Displaying event data
- Communicating with the backend API
- Client-side validation where appropriate
- Responsive and accessible user experience

## Backend

ASP.NET Core application written in C# exposing a REST API.

Responsibilities:
- Event management
- Authentication
- Authorization
- Server-side validation
- Business logic
- Database access

Entity Framework Core is planned for communication between the backend and the relational database.

## Database

PostgreSQL relational database.

The database will initially store:
- Events
- Administrative user data required for authentication

The excact database schema will be designed before implementation of the backend.

## Communication

Frontend communicates with the backend through using a REST API.

The backend communicates with PostgreSQL through Entity Framework Core.

Frontend -> REST API -> ASP.NET Core -> Entity Framwork Core -> PostgreSQL

## Public routes

- `/` - Home
- `/about` - About the actor
- `/events` - Upcoming events
- `/contact` - Contact information

## Administration routes

- `/admin` - Administration dashboard
- `/admin/login` - Administrator login

Additional administration routes may be introduced when the administration interface is designed. 


