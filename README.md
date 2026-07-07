# NOMCAT: AI-Powered Master Data Governance Engine

An enterprise-grade Master Data Governance (MDG) portal standardizing, validating, and managing material master classification data (MRO spares catalogs) to prevent duplicate database entries. Redesigned to fit the corporate identity of **Hofinsoft Technologies**, incorporating **Google Gemini AI** for semantic intelligence.

Developed by **[Vishnu633](https://github.com/Vishnu633)**.

---

## 🌟 Key Features

1. **AI Smart Classification**: Paste natural language descriptions (e.g. *"I need a 15mm steel ball bearing for plant 2"*) and Gemini auto-classifies the noun/modifier taxonomy, auto-populates attribute fields, maps the target plant, and computes a confidence score.
2. **Semantic Similarity Deduplication**: Replaced exact-hash duplication checking with text embeddings (`text-embedding-004`). Incoming requests are compared against stored vector representations, alerting users to semantic matches (e.g. matching "Carbon Steel" with "CS") and offering a "Proceed Anyway" override.
3. **Plant Extensions & Multi-Stage Workflows**: Supports SAP-standard plant extensions (extending catalog items across plant codes using a compressed 3-stage pipeline) and standard single/multiple items (4-stage pipeline).
4. **Live Data LLM NomBot**: Chat with NomBot, upgraded to a context-aware AI chatbot using `gemini-2.0-flash`. Real-time statistics and catalog data are injected directly into the chat context.
5. **Auto-Seeded Workspace**: On startup, the database auto-seeds **50 unique records** (25 bearings, 25 bolts) and request logs to provide a fully populated demo environment.
6. **Professional Dark UI**: Sleek deep-slate headers and dark-blue panel backgrounds with custom scrollbars, aligned input cards, and an interactive vertical approval timeline.

---

## 📂 Project Structure

```
├── Controllers/                 # ASP.NET Core API Endpoints (Ai, Requests, Catalog)
├── Data/                        # DBContext and Startup Database Seeder (SQLite)
├── Models/                      # C# Entity Models (ItemRequest, GoldenMasterRecord)
├── Services/                    # Core engines (GeminiService, DuplicateDetector, etc.)
├── Properties/                  # Launch settings and ports
├── Program.cs                   # App bootstrap and service registry
├── nomcat.db                    # Auto-generated SQLite Database file
└── frontend/                    # React + Vite Frontend Application
    ├── src/                     # React components, style sheets (App.css), and helpers
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
2. **(Optional) Configure Gemini API Key**:
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
   *Note: On launch, Entity Framework will automatically create the database file (`nomcat.db`) and seed the 50 unique records.*

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
2. Click **New Request** to view the **AI Smart Classification** card.
3. Paste: *"I need a 12mm steel ball bearing for plant 2"* and click **Classify with AI**. The form will auto-select `BEARING/BALL`, plant `PLT2`, and fill in the attributes.
4. Try typing a query into **NomBot** (🤖 icon bottom right) such as *"how many Ball Bearings with 35mm inside diameter?"* or *"show me golden catalog records"* to see database-aware AI responses in action.
