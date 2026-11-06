# Guía Definitiva de Defensa Oral Individual — Punto por Punto

Esta guía contiene la **explicación exacta y ubicación en el código** para responder a cada una de las 16 preguntas que el profesor te hará durante la defensa en vivo.

---

### 1. Señalar cada capa y justificar la regla de dependencia
- **Ubicación en el código:** Proyectos en la solución (`Domain`, `Application`, `Infrastructure`, `Api`).
- **Explicación:** 
  - `Domain` está en el centro y **no referencia a nadie**.
  - `Application` solo referencia a `Domain`.
  - `Infrastructure` y `Api` referencian a las capas internas.
- **¿Por qué `Application` NO referencia a `Infrastructure`?**  
  Por el **Principio de Inversión de Dependencias (D de SOLID)**. La lógica de negocio (`Application`) no debe depender de detalles de infraestructura (base de datos o librerías). `Application` define las interfaces de lo que necesita (ej. `IOrderRepository`) e `Infrastructure` las implementa. Si cambiáramos la BD de SQLite a SQL Server o PostgreSQL, la capa `Application` no sufre ningún cambio.

---

### 2. Mostrar un Command y un Query concretos con sus Handlers
- **Command en PaymentService:** [ProcessPaymentCommand.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/PaymentService/Payment.Application/Commands/ProcessPayment/ProcessPaymentCommand.cs) y su Handler [ProcessPaymentCommandHandler.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/PaymentService/Payment.Application/Commands/ProcessPayment/ProcessPaymentCommandHandler.cs).
  - *Qué hace:* Es un `IRequest<PaymentResultDto>` que recibe la orden, usuario y tarjeta. El Handler consulta la idempotencia, invoca la pasarela `MockPaymentProcessor` y guarda la transacción en `payment.db`.
- **Query en PaymentService:** [GetPaymentByOrderIdQuery.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/PaymentService/Payment.Application/Queries/GetPaymentByOrderId/GetPaymentByOrderIdQuery.cs) y su Handler [GetPaymentByOrderIdQueryHandler.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/PaymentService/Payment.Application/Queries/GetPaymentByOrderId/GetPaymentByOrderIdQueryHandler.cs).
  - *Qué hace:* Es un `IRequest<PaymentResultDto>` puramente de lectura que busca en `payment.db` el pago asociado a una orden sin modificar el estado de la BD.

---

### 3. Explicar por qué usaron MediatR y qué pasaría en el Controller sin él
- **Explicación:**  
  Usamos **MediatR** para implementar el patrón **Mediador** y desacoplar los Controllers de la lógica de negocio.
- **¿Qué pasaría si pusiéramos la lógica en el Controller?**  
  Los Controllers se convertirían en "Controllers Gordos" (*Fat Controllers*), violando el **Principio de Responsabilidad Única (S de SOLID)**. La API terminaría acoplada directamente a EF Core y a las reglas de negocio, imposibilitando reutilizar la lógica o hacer pruebas unitarias limpias.

---

### 4. Señalar dónde vive una regla de negocio en la Entidad (no en el Handler)
- **Ubicación:** [Order.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Domain/Entities/Order.cs#L35-L53) o [PaymentTransaction.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/PaymentService/Payment.Domain/Entities/PaymentTransaction.cs#L40-L54).
- **Explicación sobre el código:**  
  En `Order.cs`, los setters de las propiedades son `private set`. Las modificaciones de estado se hacen a través de métodos de dominio encapsulados como `Confirm()` y `Cancel()`:
  ```csharp
  public void Confirm()
  {
      if (Status != OrderStatus.Pending)
          throw new InvalidOrderStateException("Solo las órdenes pendientes se pueden confirmar.");
      Status = OrderStatus.Confirmed;
  }
  ```
  La regla de validación del estado vive dentro de la entidad de dominio. El Handler solo invoca el método `order.Confirm()`.

---

### 5. Mostrar que el segundo servicio también respeta Clean Architecture
- **Ubicación:** Solución `PaymentService.sln` (`Payment.Domain`, `Payment.Application`, `Payment.Infrastructure`, `Payment.Api`).
- **Explicación:**  
  Tienen exactamente la misma estructura de capas concéntricas que `ECommerce`. `Payment.Domain` contiene la entidad `PaymentTransaction`, `Payment.Application` contiene los Commands/Queries con MediatR, `Payment.Infrastructure` gestiona `PaymentDbContext` con SQLite, y `Payment.Api` expone los Controllers protegidos con JWT.

---

### 6. Mostrar el registro de HttpClient (`AddHttpClient`) y justificar `IHttpClientFactory`
- **Ubicación:** [InfrastructureServiceExtensions.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Infrastructure/InfrastructureServiceExtensions.cs#L38-L47).
- **Explicación:**
  ```csharp
  services.AddHttpClient<IPaymentClient, Services.PaymentHttpClient>(client => { ... });
  ```
- **¿Por qué `IHttpClientFactory` y NO `new HttpClient()`?**  
  Instanciar `new HttpClient()` dentro de un handler o servicio agota los sockets disponibles en el sistema operativo (*socket exhaustion*) debido al estado `TIME_WAIT` de las conexiones TCP. `IHttpClientFactory` gestiona eficientemente el ciclo de vida y la reutilización de los `HttpMessageHandler` subyacentes.

---

### 7. Señalar DTO de contrato y por qué no se expone la entidad de dominio por HTTP
- **Ubicación:** [PaymentDtos.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Application/DTOs/PaymentDtos.cs) (`ProcessPaymentRequestDto` / `PaymentResponseDto`).
- **Explicación:**  
  No exponemos entidades de dominio (`Order`, `PaymentTransaction`) por HTTP porque:
  1. Evitamos acoplar la estructura interna de la BD con el contrato público de la API.
  2. Prevenimos problemas de referencias circulares en la serialización JSON.
  3. Ocultamos campos sensibles (como contraseñas o datos internos) exponiendo únicamente los campos que el cliente necesita.

---

### 8. Recorrer una request completa en voz alta
- **Pasos a narrar:**
  1. El cliente envía `POST /api/payments/process` al controller [PaymentsController.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Api/Controllers/PaymentsController.cs).
  2. El Controller llama a `_paymentClient.ProcessPaymentAsync(request)` ([PaymentHttpClient.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Infrastructure/Services/PaymentHttpClient.cs)).
  3. `PaymentHttpClient` extrae el token JWT vía `IHttpContextAccessor` y realiza una petición HTTP `POST` a `http://localhost:5081/api/payments/process`.
  4. En `PaymentService`, la petición llega a [PaymentsController.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/PaymentService/Payment.Api/Controllers/PaymentsController.cs) que despacha `ProcessPaymentCommand` con `_mediator.Send()`.
  5. El Handler verifica idempotencia y procesa el pago con `MockPaymentProcessor`, guardando en `payment.db`.
  6. La respuesta vuelve por HTTP al ECommerce y si es aprobada, la orden se actualiza a `Confirmed`.

---

### 9. Mostrar la URL del segundo servicio (`appsettings`) y qué pasa si cambia el puerto
- **Ubicación:** [appsettings.json](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Api/appsettings.json#L19-L21) y [InfrastructureServiceExtensions.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Infrastructure/InfrastructureServiceExtensions.cs#L35-L37).
- **Explicación:**  
  La clave `"PaymentService": { "BaseUrl": "http://localhost:5081" }` se lee dinámicamente desde `appsettings.json`. Si el puerto cambia (ej. a 5090), solo se modifica este archivo de configuración sin necesidad de modificar ni recompilar código C#. Si la clave falta, el código lanza `InvalidOperationException`.

---

### 10. Qué ocurre si el segundo servicio no responde (Timeout, Try/Catch, Estado de la Orden)
- **Ubicación:** [PaymentsController.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Api/Controllers/PaymentsController.cs#L43-L60).
- **Explicación:**  
  Si `PaymentService` no responde o se cae, `PaymentHttpClient` junto con Polly (timeout de 10s y 3 reintentos) eleva una `HttpRequestException` o `TaskCanceledException`.
  El `PaymentsController` la captura con `try/catch` y devuelve un código **`502 Bad Gateway`** o **`504 Gateway Timeout`**.
  **Estado de la orden:** **Permanece en estado `Pending` (Pendiente)** en `ecommerce.db`, permitiendo al usuario reintentar el pago cuando el servicio se restablezca.

---

### 11. Cómo se genera el JWT en el login y qué claims lleva
- **Ubicación:** [JwtTokenGenerator.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Infrastructure/Auth/JwtTokenGenerator.cs#L22-L35).
- **Claims que incluye:**
  - `ClaimTypes.NameIdentifier` (`User.Id`)
  - `ClaimTypes.Email` (`User.Email`)
  - `ClaimTypes.Name` (`User.Name`)
  - `ClaimTypes.Role` (`User.Role`, ej. `"Admin"`)

---

### 12. Diferencia entre Autenticación y Autorización sobre el código
- **Autenticación (¿Quién eres?):** Se realiza en [AuthController.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Api/Controllers/AuthController.cs). Verifica que el usuario y la contraseña sean correctos mediante `BCrypt.Verify()` y emite el token JWT.
- **Autorización (¿Qué tienes permitido hacer?):** Se aplica mediante atributos como `[Authorize(Roles = "Admin")]` en los Controllers. Verifica que el token presentado por el usuario contenga el Claim de Rol necesario (`"Admin"`).

---

### 13. Mostrar endpoint protegido con `[Authorize(Roles="Admin")]`
- **Ubicación:** [ProductsController.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Api/Controllers/ProductsController.cs#L43).
- **Demostración:** Si un usuario normal (con rol `"User"`) intenta hacer `POST /api/products`, la API devuelve inmediatamente un HTTP **`403 Forbidden`**.

---

### 14. Explicar cómo se crea un usuario Admin en el sistema
- **Ubicación:** [IdentityDataSeeder.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Infrastructure/Data/IdentityDataSeeder.cs#L28-L46).
- **Explicación:**  
  Al iniciar `ECommerce.Api`, la clase `IdentityDataSeeder` consulta la base de datos. Si no existe ningún usuario con el correo `admin@ecommerce.com`, crea automáticamente la entidad `User` asignándole el rol `"Admin"` y la contraseña hasheada `Admin123!`.

---

### 15. Mostrar una migración y explicar qué generó en la base de datos
- **Ubicación:** Archivos en `ECommerce.Infrastructure/Migrations/`.
- **Explicación:**  
  La migración `InitialCreate.cs` genera las sentencias C# que traducen el modelo de EF Core a tablas SQLite (`Products`, `Categories`, `Orders`, `OrderItems`, `Users`). Al ejecutarse `db.Database.Migrate()`, crea el archivo `ecommerce.db` y la tabla especial `__EFMigrationsHistory` para registrar las migraciones aplicadas.

---

### 16. Dónde se define la interfaz del repositorio y dónde se implementa
- **Interfaz (Definición):** [IProductRepository.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Application/Interfaces/IProductRepository.cs) en la capa `Application`.
- **Implementación:** [ProductRepository.cs](file:///c:/PROYECTO--BACKEND-main%20%281%29/PROYECTO--BACKEND-main/eccomerce/ECommerce.Infrastructure/Repositories/ProductRepository.cs) en la capa `Infrastructure`.
- **¿Por qué va así?**  
  Aplica el **Principio de Inversión de Dependencias (D de SOLID)**. La capa de Aplicación declara la abstracción de lo que necesita de la BD, y la capa de Infraestructura implementa el detalle concreto usando EF Core (`ApplicationDbContext`).
