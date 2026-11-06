# Proyecto Integrador — Backend (.NET 8 Clean Architecture & Microservicios)

Este proyecto integrador contiene dos servicios independientes estructurados en **Clean Architecture**, **CQRS con MediatR**, **EF Core 8.0.11 con SQLite**, **Autenticación JWT** y **Resiliencia con Polly**.

---

## 🚀 1. Cómo levantar los dos servicios

Ejecutar los siguientes comandos desde la carpeta raíz del repositorio:

### Terminal 1: Servicio de Pagos (`PaymentService`)
```powershell
dotnet run --project PaymentService/Payment.Api/Payment.Api.csproj
```

### Terminal 2: Servicio Principal (`ECommerce.Api`)
```powershell
dotnet run --project eccomerce/ECommerce.Api/ECommerce.Api.csproj
```

---

## 🌐 2. Puertos y URLs de Swagger

| Servicio | Puerto HTTP | Documentación Swagger UI |
|---|---|---|
| **ECommerce.Api** | `http://localhost:5080` | [http://localhost:5080/swagger](http://localhost:5080/swagger) |
| **PaymentService.Api** | `http://localhost:5081` | [http://localhost:5081/swagger](http://localhost:5081/swagger) |

---

## 🔑 3. Usuario Admin de prueba (Seed en Runtime)

El usuario administrador se genera automáticamente al iniciar `ECommerce.Api` mediante `IdentityDataSeeder.cs`:

- **Email:** `admin@ecommerce.com`
- **Contraseña:** `Admin123!`
- **Rol:** `Admin`

> **Uso:** Iniciar sesión en `POST /api/auth/login` con estas credenciales, copiar el token devuelto y colocarlo en el botón **Authorize** de Swagger para ejecutar los endpoints protegidos con `[Authorize(Roles = "Admin")]` (`POST/PUT/DELETE /api/products`).

---

## 🛠️ 4. Opciones Arquitectónicas Elegidas

1. **Persistencia (Opción A - EF Core + SQLite):**
   - Ambos microservicios persisten datos de forma independiente usando **EF Core en la versión estricta `8.0.11`**.
   - `ECommerce.Api` usa `ecommerce.db` y `Payment.Api` usa `payment.db`.
2. **Seguridad en la Llamada Inter-Servicios (Con Token JWT):**
   - Se eligió **proteger el microservicio de pagos con JWT y propagar el token Bearer del usuario**.
   - `PaymentHttpClient` extrae el token recibido en la petición HTTP de `ECommerce` (vía `IHttpContextAccessor`) y lo reenvía automáticamente a `PaymentService`.
3. **Configuración de URLs (Sin Hardcodeo):**
   - La `BaseAddress` de `PaymentService` se lee estrictamente desde `appsettings.json` (`"PaymentService": { "BaseUrl": "http://localhost:5081" }`). Si falta, el sistema lanza `InvalidOperationException`.
4. **Resiliencia (Punto Extra - Polly):**
   - Configurado con `Microsoft.Extensions.Http.Polly` (8.0.11) para reintentar hasta 3 veces con tiempo de espera exponencial (2s, 4s, 8s) y un timeout de 10 segundos ante fallos de red o errores HTTP 5xx.

---

## 💡 5. Regla de Negocio del Segundo Servicio (`PaymentService`)

El servicio de pagos procesa y registra transacciones asociadas a pedidos (`OrderId`) y usuarios (`UserId`):

1. **Idempotencia:** Si la orden ya cuenta con una transacción registrada en `payment.db`, el servicio no duplica cobros y retorna el resultado existente.
2. **Validación de Tarjeta y Aprobación/Rechazo:**
   - Si el número de tarjeta ingresado finaliza en **`0000`**, el pago es **Rechazado (`PaymentStatus.Rejected`)** indicando el motivo `"Tarjeta rechazada por la pasarela de prueba (0000)"`.
   - Para montos mayores a $0 y tarjetas válidas, el pago es **Aprobado (`PaymentStatus.Approved`)** generando una referencia única de transacción (ej. `MOCK-PAY-A1B2C3D4`).
3. **Manejo de Errores e Invariantes:**
   - Si `PaymentService` está caído, `ECommerce` retorna `502 Bad Gateway`.
   - Si `PaymentService` tarda demasiado, `ECommerce` retorna `504 Gateway Timeout`.
   - En ambos casos, **la orden permanece guardada en estado `Pending` (Pendiente)** permitiendo reintentar el pago posteriormente.
