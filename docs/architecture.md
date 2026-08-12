# Architecture

## Frontend

Next.js application using React and TypeScript.

The frontend contains the following:

- Public website
- Administration interface

## Backend

Java Spring Boot application exposing a REST API.

Responsibilities:
- Event management
- Authentication
- Authorization
- Validation
- Database access

## Database

PostgreSQL relational database.

##Communication

Frontend communicates with the backend through HTTP/REST.

Frontend -> REST API -> Spring Boot -> PostgreSQL

## Public routes

- `/` - Home
- `/about` - About the actor
- `/events` - Upcoming events
- `/contact` - Contact information

## Administration routes

- `/admin` - Administration dashboard
- `/admin/login` - Administrator login



