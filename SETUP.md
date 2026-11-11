# Black Order Store — puesta en marcha

## Requisitos

- .NET SDK 10
- SQL Server (por defecto) o SQLite (desarrollo/pruebas)
- Node 20+ y Angular CLI (para el frontend en `../BlackOrder-Angular`)

## Base de datos

### SQL Server (producción)

La cadena vive en `appsettings.Development.json` (no versionado):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=BlackOrderStore;Integrated Security=True;TrustServerCertificate=True"
}
```

> Importante: `Microsoft.Data.SqlClient` (el driver de EF Core) **no usa Shared Memory**.
> Necesita TCP/IP o Named Pipes habilitados en SQL Server Configuration Manager.
> Si solo está Shared Memory, `sqlcmd` conectará pero la API fallará con error 40 (Named Pipes).

Aplicar migraciones:

```powershell
dotnet tool install --global dotnet-ef   # una vez
dotnet ef database update
dotnet run
```

### SQLite (desarrollo sin SQL Server)

```powershell
$env:Database__Provider = "Sqlite"
$env:ConnectionStrings__DefaultConnection = "Data Source=blackorder-dev.db"
dotnet run
```

El esquema se crea con `EnsureCreated` y las fechas `DateTimeOffset` se guardan
como binario solo para este proveedor (limitación de ORDER BY en SQLite).

## Primer administrador

`POST /api/User/MakeAdmin` requiere un token con claim **Admin**. Para el primer
usuario, añade el claim directamente en la base:

```sql
INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)
SELECT Id, 'Admin', 'true' FROM AspNetUsers WHERE Email = 'tu@email.com';
```

Vuelve a iniciar sesión para que el JWT incluya el claim.

## Pagos

- **Manual** (por defecto): el pedido queda `Pending` y un admin lo pasa a `Paid`.
- **Stripe** (opcional): configura `Stripe:SecretKey` (y `Stripe:Currency`). La API
  crea el `PaymentIntent` y devuelve el `ClientSecret`. El webhook de confirmación
  todavía no está implementado; el pago se confirma desde el panel admin.

## Verificación realizada

Pruebas end-to-end ejecutadas contra SQLite (misma lógica EF Core):

| Caso | Resultado |
|---|---|
| Registro y login | OK |
| Crear categoría y producto (admin) | OK |
| Aprobar producto | OK |
| Añadir al carrito + checkout | OK (total 199.98) |
| Descuento de stock | 10 → 8 |
| Mis pedidos | 1 pedido, estado Pending |
| Pasar a Paid (admin) | OK, PaymentStatus Paid |
| Dashboard admin | Ingresos 0 → 199.98, top servicio 2 uds |
| Reseña | promedio 5.0, 1 reseña |
| Favoritos | alta y baja OK |
| Pedido de otro usuario | 404 (oculto) |
| Cambio de estado por usuario | 403 |
| Crear producto sin token | 401 |
| UserList sin token / con admin | 401 / 200 |

Migraciones: `AddFavoritesTables`, `AddGuidesModule`, `AddCommerceCore` aplicadas en
SQL Server; esquema verificado (`Orders`, `OrderItems`, `Reviews`, `Products.Stock`).

## Endpoints principales

- Auth: `POST /api/User/Sign-Up`, `POST /api/User/Login`
- Catálogo: `GET /api/Product/GetProducts`, `GET /api/Product/{id}`, `GET /api/Product/Mine`
- Carrito: `GET /api/Cart/GetCart`, `POST /api/Cart/AddItem`, `DELETE /api/Cart/RemoveItem/{id}`
- Pedidos: `POST /api/Order/Checkout`, `GET /api/Order/MyOrders`, `GET /api/Order/{id}`
- Admin: `GET /api/Admin/Dashboard`, `GET /api/Admin/Products/Pending`,
  `POST /api/Admin/Products/{id}/Approve`, `GET /api/Order/Admin`, `PATCH /api/Order/{id}/Status`
- Reseñas: `GET/POST /api/Review/product/{productId}`
- Favoritos: `GET /api/Favorite`, `POST/DELETE /api/Favorite/{productId}`
- Guías: `GET /api/Guide`, `GET /api/Guide/{slug}`, `GET /api/Admin/Dashboard`
