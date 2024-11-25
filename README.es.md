## Componentes y Recursos Utilizados

| Componente                | Descripción                                             | Documentación                                                                 |
|---------------------------|---------------------------------------------------------|-------------------------------------------------------------------------------|
| Hot Chocolate             | Servidor GraphQL para .Net        | [Documentación Hot Chocolate](https://chillicream.com/docs/hotchocolate/v13)                           |
| GraphQL.Client            | Cliente Http para GraphQL        | [Documentación OpenIDDict](https://openiddict.com/)                           |
| .NET Core                 | Plataforma de desarrollo para aplicaciones modernas     | [Documentación .NET Core](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |
| BCrypt                    | Librería para encripción de contraseñas                 | [Documentación BCrypt](https://github.com/BcryptNet/bcrypt.net)               |
| Clean Architecture Template | Plantilla para arquitectura limpia en ASP.NET        | [GitHub - Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture) |


# Librería Común para los Servicios de TrackHub

La biblioteca común es un conjunto de componentes compartidos diseñados para estandarizar funcionalidades en los servicios de TrackHub, mejorando la reutilización, mantenibilidad y adherencia a los principios de Arquitectura Limpia. Basada en la [Plantilla de Arquitectura Limpia de Jason Taylor](https://github.com/jasontaylordev/CleanArchitecture), esta biblioteca organiza sus módulos en capas distintas, promoviendo una separación clara de responsabilidades y la independencia de dependencias. Este diseño no solo apoya la escalabilidad y facilidad de prueba, sino que también simplifica las actualizaciones y el mantenimiento en toda la plataforma.

## Personalizaciones Implementadas

Las siguientes personalizaciones se han implementado para satisfacer los requisitos específicos de TrackHub:

- **Autorización RBAC Mejorada**: Gestiona el control de acceso de manera segura verificando los permisos basados en roles para cada recurso.
- **Validación Personalizada de GraphQL**: Aplica validadores específicamente adaptados a las solicitudes GraphQL de TrackHub para asegurar una estructura y validación consistente.
- **Encriptación de Contraseñas con BCrypt y Certificados de Servidor**: Refuerza la encriptación de datos de usuario y secretos sensibles utilizando estándares robustos.

## Capa de Dominio

La capa de Dominio contiene los elementos centrales de la lógica de negocio, independiente de otras capas o frameworks externos. Esta separación ayuda a mantener la lógica central testeable y adaptable.

- **Constantes**: Define los metadatos clave para los modelos y la lógica de negocio en toda la aplicación. Esta estandarización ayuda a mantener un manejo de datos consistente en todos los servicios de TrackHub.
- **Extensiones**
  - **Criptográficas**: Proporciona métodos para la encriptación y desencriptación de datos sensibles, utilizando BCrypt para contraseñas de usuario y certificados de servidor para secretos como credenciales de terceros. Esta separación asegura que cada tipo de dato sensible esté protegido con el método más adecuado, mejorando la seguridad y el cumplimiento.

## Capa de Aplicación

La capa de Aplicación orquesta los casos de uso y la lógica de negocio sin conocimiento directo de la infraestructura. Gestiona el flujo de datos y aplica reglas de negocio de una manera que mantiene la lógica portátil y flexible.

### Comportamientos

- **Autorización**: Verifica el control de acceso mediante RBAC (Control de Acceso Basado en Roles), autorizando o denegando la interacción con recursos según los roles de usuario. Esto centraliza las verificaciones de acceso, mejorando la seguridad.
- **Caché**: Verifica si existe una política de caché para la solicitud, permitiendo respuestas rápidas cuando los datos están disponibles en caché. Esto reduce las llamadas a servicios externos, optimizando el rendimiento y el uso de recursos.
- **Validación de GraphQL**: Evalúa las solicitudes GraphQL para verificar que cumplan con los estándares de TrackHub, utilizando validadores para asegurar la estructura y los requisitos de datos.
- **Validación General**: Valida las solicitudes HTTP entrantes para asegurar que los datos estén completos y en el formato correcto.
- **Registro**: Graba detalles de las solicitudes entrantes para facilitar el monitoreo, el seguimiento de auditoría y la detección de problemas.
- **Excepciones No Controladas**: Captura y registra errores inesperados, ayudando al equipo de desarrollo a identificar y resolver problemas rápidamente.

## Capa de Infraestructura

La capa de Infraestructura contiene implementaciones para detalles técnicos específicos y dependencias de frameworks externos, como bases de datos, servicios externos y herramientas de seguridad.

- **Interceptors**: Gestiona automáticamente las columnas de auditoría (fechas de creación y modificación) en las tablas de la base de datos, reduciendo el esfuerzo manual y asegurando consistencia.
- **Fábrica de Clientes GraphQL**: Utiliza `HttpClientFactory` para gestionar las conexiones a servicios GraphQL, centralizando la configuración y el control de los clientes.
- **Servicio de Identidad**: Proporciona métodos de validación de identidad mediante interacciones con la API de Seguridad, asegurando autenticación centralizada y controlada. Este enfoque mejora la seguridad al permitir una integración flexible con los servicios de identidad.

## Beneficios de la Librería Común

El diseño modular de la librería se alinea con los principios de Arquitectura Limpia, asegurando que la lógica central de negocio permanezca independiente de detalles técnicos. Esta separación:

- **Mejora la Escalabilidad**: Cada servicio puede escalarse de forma independiente, y los cambios en una capa no afectan el resto del sistema.
- **Facilita la Prueba**: Las capas están aisladas, permitiendo pruebas unitarias e integradas con mínima configuración.
- **Simplifica el Mantenimiento**: La separación clara de responsabilidades permite actualizaciones fáciles y minimiza el riesgo de efectos secundarios no deseados.

## Casos de Uso de Ejemplo

Esta biblioteca es particularmente valiosa en escenarios donde los componentes reutilizables y las prácticas de seguridad consistentes son fundamentales:

- **Gestión de Identidad**: El componente de Servicio de Identidad es esencial para manejar inicios de sesión seguros en todos los servicios de TrackHub.
- **Operaciones GraphQL**: La Fábrica de Clientes GraphQL estandariza las interacciones con API, simplificando la configuración y el manejo de errores.

## Licencia

Este proyecto está bajo la Licencia Apache 2.0. Consulta el archivo [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.