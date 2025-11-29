-- Script SQL para crear la tabla de libros en Supabase
-- Ejecutar este script en el SQL Editor de Supabase

-- Crear la tabla de libros
CREATE TABLE IF NOT EXISTS libros (
    id SERIAL PRIMARY KEY,
    titulo VARCHAR(255) NOT NULL,
    autor VARCHAR(255) NOT NULL,
    isbn VARCHAR(50),
    anio_publicacion INTEGER,
    editorial VARCHAR(255),
    imagen_url TEXT,
    fecha_creacion TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    usuario_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Habilitar Row Level Security (RLS)
ALTER TABLE libros ENABLE ROW LEVEL SECURITY;

-- Eliminar políticas existentes si las hay (opcional, para evitar errores)
DROP POLICY IF EXISTS "Users can view their own books" ON libros;
DROP POLICY IF EXISTS "Users can insert their own books" ON libros;
DROP POLICY IF EXISTS "Users can update their own books" ON libros;
DROP POLICY IF EXISTS "Users can delete their own books" ON libros;

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

-- Crear índice para mejorar el rendimiento de las consultas
CREATE INDEX IF NOT EXISTS idx_libros_usuario_id ON libros(usuario_id);
