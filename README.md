# SupabaseNET - CRUD de Libros con Autenticación

Aplicación MVC de .NET que utiliza Supabase para autenticación y gestión de libros.

## Características

- ✅ Autenticación de usuarios (Login y Registro)
- ✅ CRUD completo de libros
- ✅ Subida de imágenes de libros usando Supabase Storage
- ✅ Cada usuario solo puede ver y gestionar sus propios libros
- ✅ Integración con Supabase usando el paquete NuGet oficial

## Requisitos Previos

- .NET 9.0 SDK o superior
- Una cuenta en [Supabase](https://supabase.com)
- Un proyecto creado en Supabase

## Configuración

### 1. Configurar Supabase

#### Crear la tabla de libros en Supabase

Puedes usar el archivo `setup.sql` en la raíz del proyecto o ejecutar el siguiente SQL en el SQL Editor de Supabase:

```sql
-- Crear la tabla de libros
CREATE TABLE libros (
    id SERIAL PRIMARY KEY,
    titulo VARCHAR(255) NOT NULL,
    autor VARCHAR(255) NOT NULL,
    isbn VARCHAR(50),
    anio_publicacion INTEGER,
    editorial VARCHAR(255),
    imagen_url TEXT,
    fecha_creacion TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    usuario_id UUID NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Habilitar Row Level Security (RLS)
ALTER TABLE libros ENABLE ROW LEVEL SECURITY;

-- Crear política para que los usuarios solo puedan ver sus propios libros
CREATE POLICY "Users can view their own books"
    ON libros FOR SELECT
    USING (auth.uid() = usuario_id);

-- Crear política para que los usuarios solo puedan insertar sus propios libros
CREATE POLICY "Users can insert their own books"
    ON libros FOR INSERT
    WITH CHECK (auth.uid() = usuario_id);

-- Crear política para que los usuarios solo puedan actualizar sus propios libros
CREATE POLICY "Users can update their own books"
    ON libros FOR UPDATE
    USING (auth.uid() = usuario_id)
    WITH CHECK (auth.uid() = usuario_id);

-- Crear política para que los usuarios solo puedan eliminar sus propios libros
CREATE POLICY "Users can delete their own books"
    ON libros FOR DELETE
    USING (auth.uid() = usuario_id);
```

#### Crear el bucket de Storage para imágenes

1. Ve a **Storage** en el menú lateral de Supabase
2. Haz clic en **New bucket**
3. Configura el bucket con los siguientes valores:
   - **Name**: `libros-imagenes`
   - **Public bucket**: ✅ Marcar como público (para que las imágenes sean accesibles públicamente)
   - **File size limit**: Opcional, recomendado 5MB
   - **Allowed MIME types**: Opcional, puedes restringir a `image/jpeg,image/png,image/gif,image/webp`

4. Crea las políticas de Storage para que los usuarios solo puedan subir/eliminar sus propias imágenes:

```sql
-- Política para permitir que los usuarios suban imágenes en su carpeta
CREATE POLICY "Users can upload images in their own folder"
ON storage.objects FOR INSERT
WITH CHECK (
    bucket_id = 'libros-imagenes' 
    AND (storage.foldername(name))[1] = auth.uid()::text
);

-- Política para permitir que los usuarios eliminen sus propias imágenes
CREATE POLICY "Users can delete their own images"
ON storage.objects FOR DELETE
USING (
    bucket_id = 'libros-imagenes' 
    AND (storage.foldername(name))[1] = auth.uid()::text
);

-- Política para permitir lectura pública de las imágenes
CREATE POLICY "Public images are viewable by everyone"
ON storage.objects FOR SELECT
USING (bucket_id = 'libros-imagenes');
```

### 2. Obtener las credenciales de Supabase

1. Ve a tu proyecto en Supabase
2. Ve a **Settings** > **API**
3. Copia los siguientes valores:
   - **Project URL** (ejemplo: `https://xxxxx.supabase.co`)
   - **anon public** key (la clave pública anónima)

### 3. Configurar appsettings.json

Edita el archivo `appsettings.json` y reemplaza los valores de placeholder:

```json
{
  "Supabase": {
    "Url": "TU_SUPABASE_URL",
    "Key": "TU_SUPABASE_ANON_KEY"
  }
}
```

**Ejemplo:**
```json
{
  "Supabase": {
    "Url": "https://xxxxx.supabase.co",
    "Key": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

## Ejecutar la aplicación

### Opción 1: Ejecutar directamente con .NET

1. Restaura las dependencias:
   ```bash
   dotnet restore
   ```

2. Ejecuta la aplicación:
   ```bash
   dotnet run
   ```

3. Abre tu navegador en: `https://localhost:5001` o `http://localhost:5000`

### Opción 2: Ejecutar con Docker

1. Construye la imagen:
   ```bash
   docker build -t supabasenet .
   ```

2. Ejecuta el contenedor:
   ```bash
   docker run -p 8080:8080 \
     -e SUPABASE_URL=tu_url \
     -e SUPABASE_KEY=tu_key \
     supabasenet
   ```

3. Abre tu navegador en: `http://localhost:8080`

### Opción 3: Ejecutar con Docker Compose

1. Crea un archivo `.env` con tus variables de entorno:
   ```
   SUPABASE_URL=https://tu-proyecto.supabase.co
   SUPABASE_KEY=tu_supabase_anon_key
   USE_HTTPS_REDIRECTION=false
   ```

2. Ejecuta:
   ```bash
   docker-compose up
   ```

3. Abre tu navegador en: `http://localhost:8080`

## Desplegar en PaaS

La aplicación incluye un `Dockerfile` optimizado para despliegue en plataformas como servicio (PaaS).

**Ver la guía completa de despliegue**: [DEPLOYMENT.md](./DEPLOYMENT.md)

Plataformas soportadas:
- ✅ Railway
- ✅ Render
- ✅ Heroku
- ✅ Fly.io
- ✅ DigitalOcean App Platform
- ✅ Azure Container Instances
- ✅ Google Cloud Run
- ✅ AWS App Runner / ECS Fargate

**Variables de entorno requeridas para despliegue:**
- `SUPABASE_URL`: URL de tu proyecto Supabase
- `SUPABASE_KEY`: Clave anónima de Supabase
- `PORT`: Puerto donde la aplicación escuchará (opcional, por defecto 8080)
- `USE_HTTPS_REDIRECTION`: `false` si el PaaS maneja HTTPS (opcional)

## Estructura del Proyecto

```
SupabaseNET/
├── SupabaseNET/
│   ├── Controllers/               # Controladores MVC
│   ├── Models/                    # Modelos y ViewModels
│   ├── Services/                  # Servicios de negocio
│   ├── Views/                     # Vistas Razor
│   └── Program.cs                 # Configuración
├── Dockerfile                     # Para despliegue
├── docker-compose.yml
├── setup.sql                      # Script SQL
└── README.md
```

## Uso

1. **Registrarse**: Crea una nueva cuenta desde la página de registro
2. **Iniciar Sesión**: Accede con tu email y contraseña
3. **Gestionar Libros**: Una vez autenticado, puedes:
   - Ver lista de libros (con miniaturas de imágenes)
   - Crear nuevos libros con imágenes
   - Editar libros existentes y cambiar/eliminar imágenes
   - Eliminar libros (las imágenes se eliminan automáticamente)
   - Ver detalles de un libro (con imagen completa)

## Notas Importantes

- Las credenciales de Supabase deben configurarse en `appsettings.json`
- El proyecto usa Row Level Security (RLS) en Supabase para asegurar que cada usuario solo acceda a sus propios libros
- La sesión de usuario se mantiene usando cookies de sesión de ASP.NET Core
- Los tokens de autenticación de Supabase se almacenan en la sesión del servidor

## Tecnologías Utilizadas

- ASP.NET Core MVC 9.0
- Supabase .NET Client (paquete oficial)
- Bootstrap 5 (incluido en la plantilla)
- jQuery (para validación de formularios)

## Solución de Problemas

### Error: "Las credenciales de Supabase no están configuradas"
- Asegúrate de haber configurado `Supabase:Url` y `Supabase:Key` en `appsettings.json`

### Error al crear/editar libros
- Verifica que hayas ejecutado el SQL para crear la tabla `libros` en Supabase
- Verifica que Row Level Security (RLS) esté habilitado y las políticas estén creadas

### Error de autenticación
- Verifica que la autenticación por email esté habilitada en Supabase (Settings > Authentication > Providers > Email)

### Error al subir imágenes
- Verifica que el bucket `libros-imagenes` esté creado en Supabase Storage
- Verifica que el bucket esté configurado como público
- Verifica que las políticas de Storage estén configuradas correctamente
- Verifica que el tamaño del archivo no exceda los límites configurados (recomendado: máximo 5MB)
- Formatos permitidos: JPG, JPEG, PNG, GIF, WEBP
