# Waves

1) Limitar cantidad de enemigos por wave
https://www.reddit.com/r/TheTowerGame/comments/wg8jp9/number_of_enemies/

The formula is roughly 14.9x^0.23, where x is the current wave.

This equates to:

~15 enemies on Wave 1

~ 52 enemies on Wave 100

~89 enemies on Wave 1000

~110 enemies on Wave 2500, etc.

Per my calculations, the marginal increase in enemies decreases each wave, so that the average enemies per wave approaches 100 asymptotically

2) cantidad maxima de enemigo sen Orcs Must Die

Para que te des una idea de lo "mentirosos" (en el buen sentido) que son estos juegos con la cantidad de unidades reales:

Orcs Must Die! 3: Tiene un "Hard Cap" técnico de 150 enemigos vivos simultáneamente. Aunque la oleada tenga 500 orcos en total, el juego no spawnea el 151 hasta que matas a uno de los que ya están. En los "War Scenarios" (esos mapas gigantes de guerra) el límite sube un poco (aprox 200-250), pero usan trucos de renderizado y las unidades que están lejos tienen la IA casi apagada.

Dungeon Defenders (Awakened / DD2): El límite activo ronda los 120 enemigos en pantalla. Si mirás la UI de debug o las wikis técnicas, vas a ver que rara vez pasa de 120-130.

¿Por qué tan pocos?
No es solo por rendimiento (que también), sino por Game Design.

El problema del AOE (Área de Efecto): Si ponés 1.000 unidades en pantalla, el jugador deja de usar torres de flechas (single target) y solo spamea morteros o trampas de fuego. El juego se rompe porque una sola explosión mata a 50 bichos y es demasiado eficiente. Con 100 enemigos, cada uno "vale más" y la estrategia importa.

Claridad Visual: Si hay 500 bichos pegándole a tu personaje, no entendés qué te está matando.

Pathfinding: 150 unidades calculando rutas en un laberinto cambiante (porque ponés y sacás torres) ya es costoso para la CPU.

¿Qué significa esto para vos?
Que si tu prueba técnica con 200 unidades te funcionó bien en un i5, ya estás por encima del estándar de la industria.

No te mates intentando llegar a 1.000 o 5.000 unidades tipo Ultimate Epic Battle Simulator. Un buen Tower Defense se siente "lleno" y caótico con solo 80-120 enemigos en pantalla si están bien diseñados, corren rápido y se agrupan bien.

Mi consejo: Apuntá a un límite duro de 200 activos. Si la oleada es de 1.000, andá soltándolos de a poco (spawning queue) para mantener siempre 200 vivos. Tu CPU y tus jugadores te lo van a agradecer.Para que te des una idea de lo "mentirosos" (en el buen sentido) que son estos juegos con la cantidad de unidades reales:

Orcs Must Die! 3: Tiene un "Hard Cap" técnico de 150 enemigos vivos simultáneamente. Aunque la oleada tenga 500 orcos en total, el juego no spawnea el 151 hasta que matas a uno de los que ya están. En los "War Scenarios" (esos mapas gigantes de guerra) el límite sube un poco (aprox 200-250), pero usan trucos de renderizado y las unidades que están lejos tienen la IA casi apagada.

Dungeon Defenders (Awakened / DD2): El límite activo ronda los 120 enemigos en pantalla. Si mirás la UI de debug o las wikis técnicas, vas a ver que rara vez pasa de 120-130.

¿Por qué tan pocos?
No es solo por rendimiento (que también), sino por Game Design.

El problema del AOE (Área de Efecto): Si ponés 1.000 unidades en pantalla, el jugador deja de usar torres de flechas (single target) y solo spamea morteros o trampas de fuego. El juego se rompe porque una sola explosión mata a 50 bichos y es demasiado eficiente. Con 100 enemigos, cada uno "vale más" y la estrategia importa.

Claridad Visual: Si hay 500 bichos pegándole a tu personaje, no entendés qué te está matando.

Pathfinding: 150 unidades calculando rutas en un laberinto cambiante (porque ponés y sacás torres) ya es costoso para la CPU.

¿Qué significa esto para vos?
Que si tu prueba técnica con 200 unidades te funcionó bien en un i5, ya estás por encima del estándar de la industria.

No te mates intentando llegar a 1.000 o 5.000 unidades tipo Ultimate Epic Battle Simulator. Un buen Tower Defense se siente "lleno" y caótico con solo 80-120 enemigos en pantalla si están bien diseñados, corren rápido y se agrupan bien.

Mi consejo: Apuntá a un límite duro de 200 activos. Si la oleada es de 1.000, andá soltándolos de a poco (spawning queue) para mantener siempre 200 vivos. Tu CPU y tus jugadores te lo van a agradecer.

3) yo se que en orcs must die, los personajes que estan muy lejos en el OMD3 se ven como sprites, osea.. claramente este realizado a proposito para ahorrar recursos 