# Servicio de Agronomía SmartPalm

Microservicio independiente .NET 10 del contexto delimitado de **Recomendaciones Agronómicas**. Es dueño de las recomendaciones agronómicas, su flujo de aprobación/versionado/publicación y los informes técnicos. Conserva los contratos REST públicos del monolito, pero reemplaza las llamadas directas a IAM y Crop por proyecciones locales mantenidas mediante eventos de RabbitMQ.

## Rol en la plataforma

Agronomy es la capa de apoyo a la toma de decisiones después de que están disponibles las lecturas y la salud del sector. Un PalmGrower o Agronomist puede consultar informes técnicos y recomendaciones de un sector; los Agronomists y Administrators autorizados pueden aprobarlas o publicarlas. Este servicio no captura datos de sensores ni es dueño de plantaciones, dispositivos o intervenciones.

## Propiedad y límites

- Es dueño de las raíces de agregado `Recommendation` y `TechnicalReport`.
- Es dueño del esquema privado de PostgreSQL `agronomy`.
- Consumes Identity user/subscription and Crop plantation/sector/affiliation events.
- Publica `agronomy.recommendation-*.v1` y `agronomy.report-*.v1` mediante el outbox transaccional.
- Almacena `InterventionId` únicamente como referencia externa. Las intervenciones de campo pertenecen a `SmartPalm.FieldService`.
- No lee la base de datos de otro servicio y no requiere `Code/compose.local.yaml`.

## Relaciones de integración

- **Identity Service:** proyecta usuarios, roles e información de suscripciones activas utilizada para autorización y validación del plan.
- **Crop Service:** proyecta plantaciones, sectores y afiliaciones de agrónomos para que las recomendaciones se limiten a un sector válido.
- **Ingestion Service:** no se consulta directamente. Agronomy se limita a sectores válidos y puede utilizarse junto con las vistas de salud/lecturas expuestas por la plataforma; la base de datos de Ingestion permanece independiente.
- **Field Service:** recibe o referencia intervenciones de campo mediante InterventionId; el agregado de la intervención permanece en Field.
- **Clientes web/móvil:** llaman a Agronomy a través del API Gateway, nunca directamente a la base de datos del servicio.

Las principales familias de recursos son informes y recomendaciones asociados a un sector, además de recomendaciones generales. La aprobación y publicación son transiciones de estado explícitas protegidas por políticas de roles.

## Ejecución local

Copia `.env.example` como `.env` y luego ejecuta:

```powershell
docker compose -f compose.local.yaml --env-file .env --profile isolated --profile api up --build
```

La API escucha en `http://localhost:5106` y expone su disponibilidad en `/health/ready`. Omite el perfil `isolated` cuando utilices la instancia compartida de RabbitMQ.

## Verificación

```powershell
dotnet restore SmartPalm.AgronomyService.slnx
dotnet build SmartPalm.AgronomyService.slnx --no-restore
dotnet test SmartPalm.AgronomyService.slnx --no-build
```

Las pruebas de integración requieren Docker. Configura `SKIP_DOCKER_TESTS=true` únicamente en entornos donde Docker no esté disponible de forma intencional.

## Nota sobre dependencias

No fue necesario agregar paquetes de producción aparte de las dependencias acordadas de PostgreSQL, RabbitMQ, OpenAPI y JWT. La herramienta global `dotnet-ef` instalada actualmente es 9.0.9, mientras que el servicio utiliza EF Core 10.0.9; la generación de migraciones funciona, pero se recomienda actualizar la herramienta local a la versión mayor compatible cuando esté disponible.

