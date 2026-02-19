# Desarrollar el sistema de hashmapping ==> en progreso

Estamos desarrollando un sistema de hashmapping cont x.y.z como key siendo Map<String,Object> siendo x, y ,z int e inicialmente vamos a hacer qu ela grilla sea cúbica osea.. las 3 dimensiones iguales e 5x5x5 unidades (metros)

Después vamos a calcular el valor exacto para hacer que la grilla no tenga mas de cierta cantidad de enemigos, suponiendo un caso intermedio, teniendo en cuenta los tamaños de los enemigos

No tenes que usar mas o usar lo menos posible, Area3D, StaticBody3D y PhysicsBody3D

https://www.youtube.com/watch?v=IuS-U3tDQ1c

# Ver RVO
¿Cómo lo hace Dungeon Defenders? Usan "Soft Collisions" (Colisiones Suaves) o RVO (Reciprocal Velocity Obstacles).
--------------
Paso 2: Activar la "Magia" (NavigationAgent3D)
Godot tiene el sistema de Orcs Must Die integrado. Se llama NavigationAgent3D.

Asegurate de que tu enemigo tenga un nodo NavigationAgent3D.

En el Inspector del agente, buscá la sección Avoidance.

Activá Enable Avoidance.

Dales un Radius (ej: 0.5m).

#### Detalle
- Vamos a poner Spatial Hashing para.. que los enemigos detecten los objetivos cercanos, rango de vision, radio de persecución, radio de alcance de ataque.
- Para atacar vamos a poner un collider y lo vamos a habilitar y deshabilitar, no nos quea otra me parece para evitar mandar moco a menos que desarrollemos el spatial hashing lo suficiente para poner AABB o esferas desfasadas respecto al origen o centro que establezcamos de los assets.
- Vamos a ver si podemos crear nodos con algo grafico para debuggear, y cuando no querramos debbug, borramos el nodo o lo que sea que de feedback grafico.

# Spawmeo
Hay que controlar la cantidad todal de bichos spawmeados pero vos fijate que en algunos juegos vos tenias una wave de 100 enemigos, tirabas ponele 50 enemigos y cuando ya matabas ponele 10 o 20 osea.. el chunk necesario para poder spawmear, spawmeaba un bloque mas o chunk mas no se... 10 o 20 y de esa forma vas controlando la cantida dde enemigos que tenes en la escena

# Sugerencias
- Hashmapping para la IA de la orda asi no usamos Area3D
- Hashmapping para los proyectiles de las torres
- Para los proyectiles de mi heroe arquero tendriamos que usar un raycast, obtner donde deberia de impactar el proyecil, en que punto del enemigo de la orda y de ahí obtener el punto de impacto y comparar con la altura de la capsula del enemigo, aplicar un porcentaje de altura y calcular eso y sacar si es un headshot o algo.
- Se puede hacer un sistema hibrido donde a partir de cierta distancia la hitbox sea una sola cápsula o un solo cubo, pero si está cerca ya tenes un sistema más complejo donde las hitboxes están enlazadas a lso bones así también con el tema de las animaciones, se muevan esas hitboxes y sea más preciso. Todo esto va a depender de que vamos a definir en nuestro juego

# Optimizacion
Vas a irla haciendo en la medida que la necesites, acá ya vimos que para distancias lo mejor es el spatial hashing, cuando tengamos balas volando por todos lados, las podemos poner como esferas y chau, o rectánbulas o esferas en las puntas de las flechas, eso lo vamos a ver como lo hacemos entonces podemos detectar todo.

# Flechas o balas
Para las torres tenemos que usar esferas o AABB pero por spatial hashing, chau.. no renegamos mas, o al menos obtener los cercanos por spatial hashing solo por distancias y depués de eso, al tener los enemigos cercanos agarrar y hacer el cálculo más fino.
La otra es que los enemigos mas grande so los boses o los de élite ahí si sean en muchpisima menor cantidad, y ahí capaz metemos Area3D o lo q haga falta o la lógica de componentes en vez de tipo Manager y listo, lo simplificás al sistema ya que por 10 o 20 creeps muy específicos, no pasa nada. No va a matar el rendimiento

