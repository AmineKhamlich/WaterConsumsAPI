# MEMÒRIA DEL PROJECTE: WaterConsumsAPI

## 1. Portada

*   **Nom i Cognoms:** Amine [Cognom1 Cognom2]
*   **Estudis:** CFGS Desenvolupament d'Aplicacions Multiplataforma (DAM)
*   **Centre:** Institut Rafael Campalans
*   **Títol del Projecte:** WaterConsumsAPI - Gestió de Consums d'Aigua
*   **Data:** 27 de Gener de 2026

---

## 2. Introducció i Objectius

### Introducció
El projecte "WaterConsumsAPI" es desenvolupa com a **Projecte Final del Cicle Formatiu de Grau Superior en Desenvolupament d'Aplicacions Multiplataforma (DAM)**. Consisteix en el desenvolupament d'una API RESTful per a la gestió i monitoratge de consums d'aigua en diferents plantes i punts de mesura. Aquesta aplicació permetrà centralitzar la informació dels comptadors ("DimCnt") i el seu historial de lectures ("FactCntHistorianV2").

### Objectius
Els principals objectius del projecte són:
1.  **Digitalització de Dades:** Crear un repositori centralitzat per a les dades de consum i incidències.
2.  **Gestió d'Incidències:** Facilitar la correcció d'errors i avaries en els comptadors IoT.
3.  **Arquitectura Robusta:** Implementar una arquitectura per capes (Models, DTOs, Serveis, Controladors) escalable.
4.  **Seguretat i Abstracció:** Utilitzar DTOs per protegir l'estructura interna de la base de dades.

---

## 3. Descripció General

### 3.1. Motivació
Aquest projecte neix d'una proposta i necessitat real de l'empresa on treballo. Serà una eina clau per gestionar eficientment el consum d'aigua, així com per detectar i resoldre incidències tècniques (avaries, errors de lectura) en la xarxa de comptadors IoT. La motivació principal és aportar una solució tecnològica que optimitzi el manteniment i control de la xarxa hídrica.

### 3.2. Finalitats del Projecte
El projecte serveix per:
*   Proporcionar una interfície (API) estàndard backend.
*   Donar servei a una futura aplicació mòbil Android (Kotlin) per a consultes i gestió d'incidències.
*   Registrar lectures històriques detallades i permetre la correcció de dades errònies.

### 3.3. Alternatives a la Construcció
S'ha dissenyat un sistema distribuït:
*   **Backend:** API REST amb .NET.
*   **Frontend Mòbil (Futur):** Aplicació Android nativa amb Kotlin.
*   **Base de Dades:** S'ha optat per desenvolupar i fer proves sobre una rèplica "mirall" en **MySQL** per no posar en risc les dades reals. No obstant, l'arquitectura utilitza Entity Framework per permetre una migració transparent a **SQL Server** en l'entorn industrial de producció.

### 3.4. Tria dels Llenguatges de Programació i SGBD

Per al desenvolupament s'han seleccionat les següents tecnologies:

*   **Llenguatge: C# (.NET 9):**
    *   *Motiu:* Elecció basada en l'experiència prèvia i la robustesa de l'ecosistema Microsoft. .NET 9 ofereix el rendiment necessari ("High Performance") per a entorns industrials.
    *   *Eines Clau:* Entity Framework Core (per a l'abstracció de dades) i Swagger.

*   **SGBD: MySQL (Desenvolupament) -> SQL Server (Producció):**
    *   *Estratègia:* S'utilitza MySQL en desenvolupament per seguretat (base de dades mirall). Gràcies a l'ús d'un ORM (Entity Framework), el canvi a SQL Server per al desplegament final serà, en gran mesura, un canvi de configuració, mantenint tota la lògica de negoci intacta.

---

## 4. Dietari del Projecte

A continuació es detalla el seguiment de les tasques realitzades durant el desenvolupament del projecte.

### Sessió 1: Anàlisi, Refactorització i Documentació
**Data:** 27 de Gener de 2026
**Durada:** 4 hores
**Tasques Realitzades:**
1.  **Anàlisi Inicial:** Revisió de l'estructura del projecte existent (`WConsumsAPI`). Detecció d'oportunitats de millora en l'arquitectura (acoblament fort entre Controladors i Base de Dades).
2.  **Definició de l'Arquitectura:** Disseny d'una arquitectura per capes (N-Tier) introduint DTOs i Serveis.
3.  **Implementació del Patró DTO (Data Transfer Objects):**
    *   Creació de `DimCntDto` per protegir l'entitat `DimCnt`.
    *   Adopció de `FactCntHistorianDto` per a la gestió d'històrics.
4.  **Implementació de la Capa de Serveis:**
    *   Desenvolupament de `IDimCntService` i `DimCntService` amb operacions CRUD completes.
    *   Desenvolupament de `IFactCntHistorianService` i `FactCntHistorianService`.
5.  **Refactorització de Controladors:**
    *   Modificació de `DimCntController` i `FactCntHistorianV2Controller` per injectar i utilitzar els nous serveis, eliminant l'accés directe a `DbContext`.
6.  **Correccions Tècniques:** Resolució d'errors de tipatge (int vs string en IDs) i configuració correcta de la injecció de dependències a `Program.cs` per a MySQL.
7.  **Documentació Tècnica:** Addició de comentaris exhaustius (línia a línia) a tot el codi font per facilitar-ne la comprensió i manteniment acadèmic.
8.  **Gestió del Versiomanent:** Inicialització del repositori Git, creació de `.gitignore` per a .NET i pujada del codi a GitHub (`WaterConsumsAPI`).

