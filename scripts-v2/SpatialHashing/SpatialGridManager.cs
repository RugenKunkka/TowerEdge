using Godot;
using System.Collections.Generic;

// 1. Definimos las etiquetas de las capas

public partial class SpatialGridManager:Node3D
{
    // 2. Aquí guardamos las múltiples grillas.
    // Usamos un Diccionario para mapear "Tipo de Capa" -> "Grilla Específica"
    private Dictionary<SpatialGridLayerTypeEnum, SpatialGrid<Unit>> _layers;

    public SpatialGridManager()
    {
        _layers = new Dictionary<SpatialGridLayerTypeEnum, SpatialGrid<Unit>>();

        /*
        // 3. Inicializamos cada capa por separado.
        // Fíjate que podemos darles TAMAÑOS DE CELDA DIFERENTES (Optimización!)
        
        // Suelo: Celdas de 5 metros (Unidades chicas, amontonadas)
        _layers[SpatialGridLayerTypeEnum.Ground] = new SpatialGrid<Unit>(5.0f);
        
        // Aire: Celdas de 20 metros (Aviones rápidos, más dispersos)
        _layers[SpatialGridLayerTypeEnum.Air] = new SpatialGrid<Unit>(20.0f);
        
        // Naval: Celdas de 10 metros
        _layers[SpatialGridLayerTypeEnum.Naval] = new SpatialGrid<Unit>(10.0f);
        */
        
    }

    
    public void addNewLayer(SpatialGridLayerTypeEnum spatialGridLayerTypeEnum, int cellSize)
    {
        if (_layers.ContainsKey(spatialGridLayerTypeEnum)){
            return;
        }
        _layers[spatialGridLayerTypeEnum] = new SpatialGrid<Unit>(cellSize);
    }

    // ---------------------------------------------------------
    // MÉTODOS PÚBLICOS (La fachada del edificio)
    // ---------------------------------------------------------

    // Registrar una unidad nueva
    public void AddUnit(Unit unit)
    {
        // Preguntamos a la unidad: "¿Qué eres?" (unit.Layer)
        // Y la metemos en la grilla correspondiente.
        _layers[unit.layerType].Add(unit.GlobalPosition, unit);
    }

    // Actualizar movimiento
    public void UpdateUnit(Unit unit, Vector3 oldPos, Vector3 newPos)
    {
        // Delegamos el trabajo a la grilla correcta
        _layers[unit.layerType].UpdatePosition(unit, oldPos, newPos);
    }

    // Eliminar unidad (cuando muere)
    public void RemoveUnit(Unit unit)
    {
        _layers[unit.layerType].Remove(unit.GlobalPosition, unit);
    }

    // ---------------------------------------------------------
    // LA BÚSQUEDA (La magia de las capas)
    // ---------------------------------------------------------
    
    // "Dame todos los enemigos cerca de mí"
    // Aquí puedes pedir buscar en UNA capa específica o en TODAS.
    public List<Unit> GetNearbyUnits(Vector3 position, SpatialGridLayerTypeEnum targetLayer)
    {
        // Solo buscamos en la grilla que nos interesa.
        // Si soy un Tanque antiaéreo, llamo a GetNearbyUnits(miPos, LayerType.Air)
        return _layers[targetLayer].GetObjectsAt(position);
    }
}