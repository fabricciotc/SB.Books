# Configuración de Supabase Storage para Imágenes

Este documento explica cómo configurar el bucket de Storage en Supabase para almacenar las imágenes de los libros.

## Paso 1: Crear el Bucket

1. Ve a tu proyecto en Supabase
2. En el menú lateral, selecciona **Storage**
3. Haz clic en **New bucket** o **Create a new bucket**

## Paso 2: Configurar el Bucket

Configura el bucket con los siguientes valores:

- **Name**: `libros-imagenes` (debe ser exactamente este nombre)
- **Public bucket**: ✅ **Marcar como público** (esto permite que las imágenes sean accesibles públicamente)
- **File size limit**: 5 MB (recomendado)
- **Allowed MIME types**: (opcional) `image/jpeg,image/png,image/gif,image/webp`

Haz clic en **Create bucket**

## Paso 3: Configurar Políticas de Storage

Después de crear el bucket, necesitas configurar las políticas de seguridad. Ve a **Storage** > **Policies** o ejecuta el siguiente SQL en el **SQL Editor**:

```sql
-- Política para permitir que los usuarios suban imágenes en su propia carpeta
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

## Estructura de Archivos

Las imágenes se almacenan en la siguiente estructura:
```
libros-imagenes/
  └── {user_id}/
      └── {guid}.{extension}
```

Cada usuario tiene su propia carpeta identificada por su UUID, lo que garantiza que solo puedan acceder a sus propias imágenes.

## Verificación

Para verificar que todo está configurado correctamente:

1. Intenta crear un nuevo libro con una imagen
2. Verifica que la imagen se muestre en la lista de libros
3. Verifica que la imagen se muestre en los detalles del libro
4. Intenta editar un libro y cambiar la imagen
5. Intenta eliminar un libro y verifica que la imagen también se elimine

## Solución de Problemas

### Error: "Bucket not found"
- Verifica que el bucket se llame exactamente `libros-imagenes`
- Verifica que el bucket esté creado en el proyecto correcto de Supabase

### Error: "Access denied" al subir imágenes
- Verifica que las políticas de Storage estén creadas
- Verifica que el usuario esté autenticado
- Verifica que la política permita INSERT en el bucket

### Las imágenes no se muestran
- Verifica que el bucket esté configurado como público
- Verifica que la política de SELECT esté creada
- Verifica la URL de la imagen en la consola del navegador

### Error al eliminar imágenes
- Verifica que la política de DELETE esté creada
- Verifica que el usuario esté intentando eliminar sus propias imágenes
