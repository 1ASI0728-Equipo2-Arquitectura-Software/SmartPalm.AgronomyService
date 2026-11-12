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

## Diagnóstico asistido de palma aceitera

`POST /api/v1/sectors/{sectorId}/recommendations/diagnosis` recibe lecturas de prueba proporcionadas por el cliente, las agrupa por variable y unidad y solicita a MiniMax un informe en español. No consulta automáticamente Ingestion ni SQLite, no guarda el resultado y no publica recomendaciones. El usuario debe autenticarse como agrónomo o administrador y cumplir las reglas actuales de suscripción y afiliación al sector.

Configura `MINIMAX_API_KEY` y, opcionalmente, `MINIMAX_MODEL` como variables de entorno del proceso o del contenedor. El modelo predeterminado es `MiniMax-M3.1-Flash`. El archivo privado de configuración no se carga automáticamente por .NET; Docker Compose debe recibir sus variables al arrancar. No coloques claves en ejemplos, código ni commits. Si el proveedor no está disponible, el endpoint responde `502` sin devolver detalles de la credencial.

Ejemplo de cuerpo para un sector de prueba, con fechas UTC y unidades del CSV:

```json
{
  "from": "2026-06-18T00:00:00Z",
  "to": "2026-06-19T00:00:00Z",
  "readings": [
    { "variable": "temperature", "value": 27.5, "unit": "C", "measuredAt": "2026-06-18T10:00:00Z" },
    { "variable": "humidity", "value": 58, "unit": "%", "measuredAt": "2026-06-18T10:00:00Z" },
    { "variable": "soilMoisture", "value": 3, "unit": "%", "measuredAt": "2026-06-18T10:00:00Z" },
    { "variable": "light", "value": 65, "unit": "lux", "measuredAt": "2026-06-18T10:00:00Z" }
  ]
}
```

La solicitud admite entre 1 y 10.000 lecturas y debe incluir `from` anterior a `to`. Las lecturas fuera del periodo se excluyen y cuentan en el resumen; se requiere al menos una dentro del periodo. Luz en `%` y en `lux` se resume por separado. El informe es orientativo y requiere revisión profesional; no prescribe riego automático ni tratamientos.

## Verificación

```powershell
dotnet restore SmartPalm.AgronomyService.slnx
dotnet build SmartPalm.AgronomyService.slnx --no-restore
dotnet test SmartPalm.AgronomyService.slnx --no-build
```

Las pruebas de integración requieren Docker. Configura `SKIP_DOCKER_TESTS=true` únicamente en entornos donde Docker no esté disponible de forma intencional.

## Nota sobre dependencias

No fue necesario agregar paquetes de producción aparte de las dependencias acordadas de PostgreSQL, RabbitMQ, OpenAPI y JWT. La herramienta global `dotnet-ef` instalada actualmente es 9.0.9, mientras que el servicio utiliza EF Core 10.0.9; la generación de migraciones funciona, pero se recomienda actualizar la herramienta local a la versión mayor compatible cuando esté disponible.

