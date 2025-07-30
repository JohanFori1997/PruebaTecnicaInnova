# PruebaTecnicaInnova

1. Resumen del Proyecto

Este proyecto implementa una API RESTful simple utilizando ASP.NET Core para gestionar un recurso Producto. La solución está diseñada para ser modular y extensible, siguiendo principios de buenas prácticas. Incluye una simulación de almacenamiento de datos en memoria y una demostración de consultas y pruebas unitarias.

2. Estructura del Proyecto

La solución se organiza en las siguientes capas lógicas:
PruebaTecnicaInnova.Models: Contiene las definiciones de las entidades de datos, como la clase Product, incluyendo validaciones básicas a través de Data Annotations.
PruebaTecnicaInnova.Repositories: Define la interfaz IProductRepository y su implementación InMemoryProductRepository. Esta capa es responsable del acceso y la gestión de los datos (simulados en memoria), desacoplando la lógica de negocio de la persistencia.
PruebaTecnicaInnova.Controllers: Contiene el ProductsController, que expone los endpoints HTTP de la API RESTful. Utiliza la interfaz IProductRepository a través de inyección de dependencias.
PruebaTecnicaInnova.Tests: Proyecto de pruebas unitarias para validar la lógica de negocio.

3. Tecnologías Utilizadas

ASP.NET Core: Framework para la construcción de la API RESTful.
C#: Lenguaje de programación.
xUnit: Framework de pruebas unitarias.
Swagger: Para la documentación interactiva de la API.

4. Características Implementadas


4.1. API RESTful de Productos

La API expone los siguientes endpoints para el recurso Producto:
GET /api/Products: Obtiene una lista de todos los productos disponibles.
GET /api/Products/{id}: Obtiene un producto específico por su Id.
POST /api/Products: Crea un nuevo producto. Espera un objeto Product en el cuerpo de la solicitud.
PUT /api/Products/{id}: Actualiza un producto existente. Espera el Id del producto en la URL y el objeto Product actualizado en el cuerpo.
DELETE /api/Products/{id}: Elimina un producto por su Id.

4.2. Validaciones Básicas

Se han implementado validaciones básicas en el modelo Product utilizando Data Annotations para asegurar que el Name y el Price no sean nulos y cumplan con rangos y longitudes específicas. Estas validaciones son automáticamente procesadas por el framework ASP.NET Core.

4.3. Almacenamiento en Memoria

Los datos de los productos se almacenan en una lista estática en memoria (InMemoryProductRepository), lo que simula una base de datos y permite un rápido desarrollo y prueba sin necesidad de configurar una base de datos real. El repositorio incluye una carga inicial de datos de ejemplo.

4.4. Simulación de Consulta a Base de Datos

Se ha implementado el método GetProductsByMinPriceAsync(decimal minPrice) dentro de InMemoryProductRepository. Este método simula una consulta a una base de datos, devolviendo todos los productos cuyo Precio es mayor o igual al minPrice especificado.

4.5. Pruebas Unitarias
Se ha creado un proyecto de pruebas unitarias (PruebaTecnicaInnova.Tests) utilizando xUnit para validar la lógica del método GetProductsByMinPriceAsync. Las pruebas siguen el patrón AAA (Arrange, Act, Assert) y cubren escenarios de éxito, casos límite (ej. cuando no hay productos que cumplan el criterio o el precio mínimo es cero) y casos de borde.

5. Decisiones de Diseño Clave

Inyección de Dependencias (DI): Se utiliza la DI para desacoplar el ProductsController del InMemoryProductRepository a través de la interfaz IProductRepository. Esto facilita la sustitución del almacenamiento en memoria por una base de datos real en el futuro y mejora la capacidad de prueba.
Programación Asíncrona (async/await): Todas las operaciones del repositorio son asíncronas (Task), lo que simula el comportamiento de operaciones I/O reales con una base de datos y mejora la escalabilidad de la API.
Separación de Responsabilidades: El código está organizado en capas claras (Modelos, Repositorios, Controladores), lo que mejora la mantenibilidad, legibilidad y testabilidad del proyecto.
Códigos de Estado HTTP: La API utiliza códigos de estado HTTP estándar (ej. 200 OK, 201 Created, 204 No Content, 400 Bad Request, 404 Not Found) para indicar el resultado de las operaciones, siguiendo los principios RESTful.

6. Cómo Ejecutar la Solución

Clonar el Repositorio:
git clone https://github.com/JohanFori1997/PruebaTecnicaInnova.git
cd PruebaTecnicaInnova
Restaurar Dependencias:
dotnet restore
Ejecutar la API:
dotnet run --project PruebaTecnicaInnova
La API estará disponible por defecto en https://localhost:44376 (el puerto puede variar).
Acceder a Swagger UI: Una vez que la API esté corriendo, se puede acceder a la documentación interactiva en https://localhost:44376/swagger/index.html para probar los endpoints.
Ejecutar las Pruebas Unitarias:
dotnet test PruebaTecnicaInnova.Tests

7. Próximos Pasos Potenciales

Integración con una base de datos real (ej. SQL Server, PostgreSQL) utilizando Entity Framework Core.
Implementación de manejo de errores global (middleware de excepciones).
Adición de autenticación y autorización.
Logging y monitoreo.
