# NOMCAT: AI-Powered Master Data Governance Engine

An enterprise-grade Master Data Governance (MDG) portal standardizing, validating, and managing material master classification data (MRO spares catalogs) to prevent duplicate database entries. Redesigned to fit the corporate identity of **Hofinsoft Technologies**, incorporating **Google Gemini AI** for semantic intelligence.

Developed by **[Vishnu633](https://github.com/Vishnu633)**.

---

## 🌟 Key Features & Recent Upgrades

1. **AI Smart Classification**: Paste natural language descriptions (e.g., *"I need a 15mm steel ball bearing for plant 2"*) and Gemini auto-classifies the noun/modifier taxonomy, auto-populates attribute fields, maps the target plant, and computes a confidence score.
2. **AI Data Steward Compliance Auditing**: A built-in virtual steward that validates engineering specifications, safety regulations, and nomenclature compliance (e.g., matching pressure ratings with connection types) and generates a clean, structured compliance checklist report.
3. **NomBot Inline Visual Analytics**: A context-aware chatbot using `gemini-flash-lite-latest` that interacts with live database statistics. When asked about metrics, plant split, or pipeline statuses, NomBot renders **interactive SVG and CSS visual charts** (Donut, Horizontal Bar, and Lifecycle Funnel) directly in the chat bubbles.
4. **Legacy Catalog Batch Auto-Cleanser**: Bulk import interface upgraded with an AI auto-cleanser. Users paste raw, unstandardized shorthand legacy lists, and the system processes them in parallel to yield parsed attributes and clean nomenclature grids.
5. **Advanced Filtering & Custom Exports**: The Reporting center supports custom three-way dropdown filtering (Plant, Noun, Status) mapping to an interactive staging ledger preview table, supporting custom CSV reports and formatted physical print views.
6. **Semantic Similarity Deduplication**: Replaced exact-hash duplication checking with text embeddings (`text-embedding-004`). Incoming requests are compared against stored vector representations, alerting users to semantic matches (e.g., matching "Carbon Steel" with "CS") and offering a "Proceed Anyway" override.
7. **Plant Extensions & Multi-Stage Workflows**: Supports SAP-standard plant extensions (extending catalog items across plant codes using a compressed 3-stage pipeline) and standard single/multiple items (4-stage pipeline).
8. **Auto-Seeded Workspace**: On startup, the database auto-seeds request logs and **102 unique catalog records** to provide a fully populated demo environment.

---

## 📂 Project Structure

```
├── Controllers/                 # ASP.NET Core API Endpoints (Ai, Requests, Catalog, Reporting)
├── Data/                        # DBContext and Startup Database Seeder (SQLite)
├── Models/                      # C# Entity Models (ItemRequest, GoldenMasterRecord)
├── Services/                    # Core engines (GeminiService, DuplicateDetector, NomBotService)
├── Properties/                  # Launch settings and ports
├── nomcat.db                    # Auto-generated SQLite Database file
└── frontend/                    # React + Vite Frontend Application
    ├── src/                     # React components, style sheets (App.css), and helpers
    ├── package.json             # NPM dependencies
    └── vite.config.js           # Vite dev server configuration
```

---

## 🚀 Setup & Execution Guide (Cross-Platform)

### Prerequisites
Make sure your computer has the following tools installed:
1. **[.NET 10.0 SDK](https://dotnet.microsoft.com/download)**
2. **[Node.js](https://nodejs.org/)** (v18+ recommended) with npm

---

### Step 1: Run the Backend API

1. Open your terminal at the project root directory.
2. **Configure Gemini API Key**:
   To enable AI features, set the `GEMINI_API_KEY` environment variable. If not set, NomBot and deduplication will run using local fallback matching logic.
   * **macOS / Linux**:
     ```bash
     export GEMINI_API_KEY="your_api_key_here"
     ```
   * **Windows (Command Prompt)**:
     ```cmd
     set GEMINI_API_KEY=your_api_key_here
     ```
   * **Windows (PowerShell)**:
     ```powershell
     $env:GEMINI_API_KEY="your_api_key_here"
     ```
3. Run the backend web server:
     ```bash
     dotnet run
     ```
   *The server will start and listen on **`http://localhost:5181`**.*
   *Note: On launch, Entity Framework will automatically create the database file (`nomcat.db`) and seed the catalog records.*

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
2. Select a role (e.g., **Sarah J. - Requester**) and click **Sign In**.
3. Navigate to **New Request** to view the **AI Smart Classification** card.
4. Try pasting: *"I need a 12mm steel ball bearing for plant 2"* and click **Classify with AI**. The form will auto-select `BEARING/BALL`, plant `PLT2`, and fill in the attributes.
5. Under the **AI Data Steward Audit** card, click **Run AI Audit** to verify safety compliance.
6. Try typing a query into **NomBot** (🤖 icon bottom right) such as *"explain the analytics and insights graphs and show me the charts"* to see visual inline charts (Donut, Bar, Funnel) rendered directly inside the chat.
