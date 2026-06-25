# Master Data Governance Demo (eNOMCAT Portal)

An enterprise-grade Master Data Governance (MDG) portal prototype built to standardize, validate, and manage material master classification data (MRO spares catalogs) and prevent duplicate database entries. Designed to fit the corporate identity and solution requirements of **Hofinsoft Technologies**.

Developed by **[Vishnu633](https://github.com/Vishnu633)**.

---

## 🌟 Key Features

1. **Brand-Aligned UI**: Fully aligned with the Hofinsoft corporate brand aesthetic using clean light-mode cards, Hofinsoft Blue (`#004884`), and Hofinsoft Orange (`#f26522`) hover accents.
2. **Data Dictionary & Dynamic Forms**: Seeds classification parameters. Selecting a Noun (e.g. `VALVE`, `MOTOR`, `GASKET`, `BEARING`) and Modifier (e.g. `BALL`, `GATE`, `INDUCTION`, `SPIRAL_WOUND`) dynamically renders the required catalog inputs (e.g. Size, Pressure Class, Material Grade, Power Rating) on-screen.
3. **Nomenclature Generator**: Automatically structures material catalog properties into a standardized description nomenclature, complying with ERP classification formats.
4. **Similarity & Deduplication Engine**: Calculates a deterministic SHA-256 hash based on sorted properties. If a duplicate unique ID exists in the Production Catalog, the system blocks the request at the gate to prevent master data pollution.
5. **Gated Staging Workflow**: Implements a staging request framework. Submissions sit as `Pending` until promoted by an administrator, mocking Camunda workflow orchestration steps before creating the Golden Master Record.

---

## 📂 Project Structure

```
├── Controllers/                 # ASP.NET Core API Endpoints
├── Data/                        # DBContext and EF Core Initializers (SQLite)
├── Models/                      # C# Entity Models (Templates, Staging, Catalog)
├── Services/                    # Camunda Workflow Orchestrator Mock Service
├── Properties/                  # Launch settings and ports
├── Appsettings.json             # DB connection configuration
├── Program.cs                   # App bootstrap and CORS policy
├── Hofinsoft_MDG_Presentation.pptx # Generated Slide Deck for presentation
├── hofinsoft_brand_e2e_...webp  # WebP E2E Walkthrough Recording
├── README.md                    # Project documentation
├── .gitignore                   # Git exclusion rules
└── frontend/                    # React 19 + Vite Frontend Application
    ├── src/                     # React components, style sheets, and helpers
    ├── package.json             # NPM dependencies
    └── vite.config.js           # Vite dev server configuration
```

---

## 🚀 Setup & Execution Guide

### Prerequisites
Make sure your computer has the following tools installed:
1. **[.NET 10.0 SDK](https://dotnet.microsoft.com/download)**
2. **[Node.js](https://nodejs.org/)** (v18+ recommended) with npm

---

### Step 1: Run the Backend API

1. Open your terminal at the project root directory.
2. Build the project to verify dependencies:
   ```bash
   dotnet build
   ```
3. Run the backend Web API:
   ```bash
   dotnet run --launch-profile http
   ```
   *The server will start and listen on **`http://localhost:5181`**.*
   *Note: On launch, Entity Framework Core will automatically create and seed the SQLite database file (`mdg.db`) if it is not already present.*

---

### Step 2: Run the React Frontend Portal

1. Open a new terminal window and navigate to the `frontend` subdirectory:
   ```bash
   cd frontend
   ```
2. Install npm package dependencies:
   ```bash
   npm install
   ```
3. Launch the Vite development server:
   ```bash
   npm run dev
   ```
   *The React client will launch and run on **`http://localhost:5173/`**.*

---

### Step 3: Run the Demo in Your Browser

1. Open your browser and navigate to **`http://localhost:5173/`**.
2. Select **Noun**: `VALVE` and **Modifier**: `BALL` to see the dynamic inputs rendered.
3. Fill in the values:
   * **Size**: `2 inches`
   * **Pressure Class**: `Class 300`
   * **Body Material**: `Carbon Steel`
   * **End Connection**: `Flanged`
4. Click **Submit Governance** to save the item as a staged pending request.
5. Navigate to the **Staging Board** tab and click **Approve** to promote the Valve to the **Production Golden Records Catalog** with its unique SHA-256 spec hash.
6. Try submitting the exact same parameters a second time to see the deduplication engine in action blocking the duplicate with the message **"Duplicate entry blocked by governance logic."**
