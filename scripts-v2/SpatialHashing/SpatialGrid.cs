using Godot;
using System;
using System.Collections.Generic;

//Spatial Grid o SpatialHashing le podemos llamar
public partial class SpatialGrid<T>
{
	
	// Tamaño de la celda (ej: 10 metros).
    private int _cellSize;

	// la key va a ser un Vector3I con coordenadas x.y.z
	//como Vector3I es un struct, compara el contenido y no la direccon de memoria
    // Tu estructura solicitada (pero mejorada con T en vez de Object).
    private Dictionary<Vector3I, List<T>> _gridObjectsDictionary;

    // CONSTRUCTOR
    public SpatialGrid(int cellSize)
    {
        _cellSize = cellSize;
        _gridObjectsDictionary = new Dictionary<Vector3I, List<T>>();
    }

    // ---------------------------------------------------------
    // 1. MATEMÁTICA: Convertir Mundo (float) a Grilla (int)
    // ---------------------------------------------------------
    public Vector3I GetGridPosition(Vector3 worldPosition)
    {
        // Dividimos por el tamaño y redondeamos hacia abajo (Floor).
        int x = Mathf.FloorToInt(worldPosition.X / _cellSize);
        // Ojo: En 3D RTS, a veces ignoramos Y (altura) si es plano. 
        // Si tu juego tiene pisos verticales, deja la Y. Si es plano, pon 0.
        int y = Mathf.FloorToInt(worldPosition.Y / _cellSize); 
        int z = Mathf.FloorToInt(worldPosition.Z / _cellSize);

        return new Vector3I(x, y, z);
    }

    // ---------------------------------------------------------
    // 2. INSERTAR: Agregar una unidad
    // ---------------------------------------------------------
    public void Add(Vector3 worldPosition, T item)
    {
        Vector3I cellKey = GetGridPosition(worldPosition);

        // Si la celda no existe todavía, creamos la lista nueva.
        if (!_gridObjectsDictionary.ContainsKey(cellKey))
        {
            _gridObjectsDictionary[cellKey] = new List<T>();
        }

        // Agregamos el item a la lista de esa celda.
        _gridObjectsDictionary[cellKey].Add(item);
    }

    // ---------------------------------------------------------
    // 3. ELIMINAR: Quitar una unidad (Necesitas saber dónde estaba)
    // ---------------------------------------------------------
    public void Remove(Vector3 worldPosition, T item)
    {
        Vector3I cellKey = GetGridPosition(worldPosition);

        // Verificamos si la celda existe para evitar errores
        if (_gridObjectsDictionary.ContainsKey(cellKey))
        {
            List<T> list = _gridObjectsDictionary[cellKey];
            
            // Removemos el item (O(n) en la lista pequeña)
            list.Remove(item);

            // OPTIMIZACIÓN IMPORTANTE:
            // Si la lista queda vacía, borramos la entrada del diccionario.
            // Esto mantiene el Dictionary pequeño y rápido.
            if (list.Count == 0)
            {
                _gridObjectsDictionary.Remove(cellKey);
            }
        }
    }

    // ---------------------------------------------------------
    // 4. MOVER: Actualizar posición (El truco del RTS)
    // ---------------------------------------------------------
    public void UpdatePosition(T item, Vector3 oldPos, Vector3 newPos)
    {
        Vector3I oldKey = GetGridPosition(oldPos);
        Vector3I newKey = GetGridPosition(newPos);

        // Solo hacemos el trabajo pesado si cambió de celda.
        // Si se movió 1 metro y sigue en la misma celda, no hacemos NADA (Ahorro CPU).
        if (oldKey != newKey)
        {
            // Quitamos de la vieja
            Remove(oldPos, item);
            // Agregamos a la nueva
            Add(newPos, item);
        }
    }

    // ---------------------------------------------------------
    // 5. CONSULTAR: ¿Que objetos estan en esa posicion de la grilla?
    // ---------------------------------------------------------
    public List<T> GetObjectsAt(Vector3 worldPosition)
    {
        Vector3I cellKey = GetGridPosition(worldPosition);

        if (_gridObjectsDictionary.ContainsKey(cellKey))
        {
            return _gridObjectsDictionary[cellKey];
        }

        // Si no hay nada, devolvemos null o una lista vacía (según prefieras).
        // Devolver null es más rápido (no crea objeto), pero requiere checkear null afuera.
        return null; 
    }
    
    // DEBUG: Para ver cuántas celdas activas hay
    public int GetActiveCellsCount() => _gridObjectsDictionary.Count;

	
}
