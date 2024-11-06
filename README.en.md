## Components and Resources

| Component                | Description                                           | Documentation                                                                 |
|--------------------------|-------------------------------------------------------|-------------------------------------------------------------------------------|
| Hot Chocolate            | GraphQL server for .NET                               | [Hot Chocolate Documentation](https://chillicream.com/docs/hotchocolate/v13)  |
| GraphQL.Client           | HTTP client for GraphQL                               | [OpenIDDict Documentation](https://openiddict.com/)                           |
| .NET Core 8              | Development platform for modern applications          | [.NET Core 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8/overview) |
| BCrypt                   | Library for password encryption                       | [BCrypt Documentation](https://github.com/BcryptNet/bcrypt.net)               |
| Clean Architecture Template | Template for clean architecture in ASP.NET         | [GitHub - Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture) |


# Common Library for TrackHub Services

The common library is a shared set of components designed to standardize functionality across TrackHub services, improving reusability, maintainability, and adherence to Clean Architecture principles. Based on [Jason Taylor’s Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture), this library organizes modules into distinct layers, which promotes clear separation of responsibilities and dependency independence. This design not only supports scalability and ease of testing but also simplifies updates and maintenance across the platform.

## Customizations Implemented

The following customizations have been implemented to meet TrackHub’s unique requirements:

- **Enhanced RBAC Authorization**: Securely manages access control by checking role-based permissions for each resource.
- **Custom GraphQL Validation**: Applies validators specifically tailored to TrackHub’s GraphQL requests to ensure consistent query structure and validation.
- **BCrypt Password Encryption and Server Certificates**: Enforces strong encryption for user data and sensitive secrets using robust standards.

## Domain Layer

The Domain layer contains the core elements of the business logic, independent of other layers or external frameworks. This isolation helps keep the core logic testable and adaptable.

- **Constants**: Defines key metadata for models and business logic throughout the application. This standardization helps maintain consistent data handling across all TrackHub services.
- **Extensions**
  - **Cryptographic**: Provides methods for encrypting and decrypting sensitive data, using BCrypt for user passwords and server certificates for secrets like third-party credentials. This separation ensures that each type of sensitive data is protected using the most suitable method, improving security and compliance.

## Application Layer

The Application layer orchestrates use cases and business logic without direct knowledge of the infrastructure. It manages data flow and applies business rules in a way that keeps logic portable and flexible.

### Behaviors

- **Authorization**: Checks access control through RBAC (Role-Based Access Control), authorizing or denying interaction with resources based on user roles. This centralizes access checks, enhancing security.
- **Caching**: Checks if a cache policy exists for the request, allowing quick responses when data is available in the cache. This reduces external service calls, optimizing performance and resource use.
- **GraphQL Validation**: Evaluates GraphQL requests for compliance with TrackHub’s standards, using validators to enforce structure and data requirements.
- **General Validation**: Validates incoming HTTP requests to ensure data is complete and correctly formatted.
- **Logging**: Records details of incoming requests to facilitate monitoring, audit trails, and issue tracking.
- **Unhandled Exceptions**: Captures and logs unexpected errors, which helps the development team identify and resolve issues quickly.

## Infrastructure Layer

The Infrastructure layer contains implementations for technical specifics and dependencies on external frameworks, such as databases, external services, and security tools.

- **Interceptors**: Automatically manages audit columns (created and modified dates) in database tables, reducing manual effort and ensuring consistency.
- **GraphQL Client Factory**: Uses `HttpClientFactory` to manage connections to GraphQL services, centralizing configuration and control of clients.
- **Identity Service**: Provides identity validation methods by interacting with the Security API, ensuring centralized and controlled authentication. This approach enhances security while allowing flexible integration with identity services.

## Benefits of the Common Library

The library’s modular design aligns with Clean Architecture principles, ensuring that core business logic remains independent from technical details. This separation:

- **Improves Scalability**: Each service can scale independently, and changes in one layer do not cascade through the system.
- **Facilitates Testing**: Layers are isolated, enabling unit and integration testing with minimal setup.
- **Simplifies Maintenance**: Clear separation of concerns allows easier updates and minimizes the risk of unintended side effects.

## Example Use Cases

This library is particularly valuable in scenarios where reusable components and consistent security practices are crucial:

- **Identity Management**: The Identity Service component is essential for handling secure logins across all TrackHub services.
- **GraphQL Operations**: The GraphQL Client Factory standardizes API interactions, simplifying configuration and error handling.

## License

This project is licensed under the Apache 2.0 License. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.