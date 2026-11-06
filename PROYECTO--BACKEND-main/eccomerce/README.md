# E-Commerce API — Proyecto Integrador

API REST en **.NET 8** con **Clean Architecture**, **Entity Framework Core 8.0.11**, **SQLite**, **CQRS** (Command + Query), **JWT** y **Swagger**.

**Asignatura:** Backend (Optativa II) — Universidad Católica de Cuyo

---

## 🚀 1. Cómo levantar los dos servicios

```powershell
# Si tu terminal está dentro de la carpeta 'eccomerce':
dotnet run --project ECommerce.Api/ECommerce.Api.csproj

# Si tu terminal está en la carpeta raíz del proyecto:
# Terminal 1: PaymentService (Puerto 5081)
dotnet run --project PaymentService/Payment.Api/Payment.Api.csproj

# Terminal 2: ECommerce.Api (Puerto 5080)
dotnet run --project eccomerce/ECommerce.Api/ECommerce.Api.csproj
```

---

## 🌐 2. Puertos HTTP y Swagger

- **ECommerce.Api:** `http://localhost:5080` | Swagger: [http://localhost:5080/swagger](http://localhost:5080/swagger)
- **PaymentService.Api:** `http://localhost:5081` | Swagger: [http://localhost:5081/swagger](http://localhost:5081/swagger)

---

## 🔑 3. Usuario Admin de prueba

- **Email:** `admin@ecommerce.com`
- **Contraseña:** `Admin123!`
- **Rol:** `Admin` (Sembrado automáticamente en inicio vía `IdentityDataSeeder.cs`)

---

## 🛠️ 4. Opciones Elegidas

- **Persistencia:** EF Core 8.0.11 con SQLite (`ecommerce.db` y `payment.db`).
- **Llamadas Inter-Servicios:** Protegidas con JWT mediante propagación de token Bearer en `HttpClient` (`IHttpContextAccessor`).
- **Resiliencia (Punto Extra):** Polly con 3 reintentos exponenciales (2s, 4s, 8s) y timeout de 10s.
- **Configuración:** `PaymentService:BaseUrl` leída desde `appsettings.json` sin hardcodeo en código.

---

## 💡 5. Regla de Negocio del Servicio de Pagos (`PaymentService`)

- Procesa pagos verificando idempotencia por `OrderId`.
- Tarjetas terminadas en `0000` se **Rechazan (`PaymentStatus.Rejected`)**.
- Demás tarjetas con monto > 0 se **Aprueban (`PaymentStatus.Approved`)** generando `MOCK-PAY-XXXX`.
- Si el servicio está caído o en timeout, la orden en ECommerce **permanece en estado `Pending`** y se retorna respuesta HTTP 502/504 al cliente.
