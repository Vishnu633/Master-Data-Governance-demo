# Implementation Plan - Master Data Governance (MDG) Material Cataloging System

This document outlines the proposed changes to ensure robust integration between the .NET Core governance API and the React-based frontend portal.

## User Review Required

No major architectural modifications are introduced, but we are standardizing the API error responses to return JSON objects (e.g. `new { Message = "..." }`) instead of raw text strings. This avoids client-side JSON parsing errors when the server reports governance validation or duplication blocks.

## Open Questions

No open questions. The requirements are clear, and the boilerplate is mostly functional.

---

## Proposed Changes

### Backend Controllers

We will standardize all `BadRequest(...)` actions returning raw strings to instead return JSON objects containing a `Message` property.

#### [MODIFY] [GovernanceController.cs](file:///Users/vishnumenon/Desktop/Spring/%2026/Hofinsoft/Controllers/GovernanceController.cs)
- Line 31: Change `return BadRequest("Noun and Modifier are required.");` to `return BadRequest(new { Message = "Noun and Modifier are required." });`
- Line 43: Change template error message to JSON object.
- Line 76: Change duplication check error to JSON object: `return BadRequest(new { Message = "Duplicate entry blocked by governance logic." });`

#### [MODIFY] [StagingController.cs](file:///Users/vishnumenon/Desktop/Spring/%2026/Hofinsoft/Controllers/StagingController.cs)
- Line 53, 65, 79, 97, 108, 119, 133: Wrap all raw string `BadRequest` returns in a JSON object with a `Message` property.

#### [MODIFY] [MaterialTemplatesController.cs](file:///Users/vishnumenon/Desktop/Spring/%2026/Hofinsoft/Controllers/MaterialTemplatesController.cs)
- Line 34: Change raw string `BadRequest` to standard JSON object.

---

### Frontend Components

We will introduce a `safeParseResponse` utility in the React client to gracefully parse both JSON and text responses from the API without throwing a `SyntaxError` exception.

#### [MODIFY] [App.jsx](file:///Users/vishnumenon/Desktop/Spring/%2026/Hofinsoft/frontend/src/App.jsx)
- Implement `safeParseResponse(res)` helper function.
- Replace direct `await res.json()` calls with `await safeParseResponse(res)` for operations that can return non-JSON responses.

---

## Verification Plan

### Automated Tests & Compilations
- Run `dotnet build` on the backend to verify compiling status.
- Start backend: `dotnet run` (listens on `http://localhost:5181`).
- Start frontend: `npm run dev` in `/frontend` directory.

### Manual Verification (End-to-End Walkthrough)
1. Launch integrated browser instance.
2. Select **Noun: BEARING** and **Modifier: BALL** from dropdowns.
3. Observe dynamic fields generated: `Inside Diameter`, `Outside Diameter`, and `Material Grade`.
4. Enter unique details:
   - Inside Diameter: `25mm`
   - Outside Diameter: `52mm`
   - Material Grade: `Stainless Steel`
5. Click **Submit Governance**. Verify request moves to the Staging Board as **Pending**.
6. Switch to Staging Board, verify the generated standardized nomenclature: `BEARING, BALL: 25mm ID, 52mm OD, Stainless Steel`.
7. Click **Approve** and verify it is promoted to the **Production Golden Records** catalog.
8. Re-attempt to submit the exact same details and verify the submission is blocked with the message: **"Duplicate entry blocked by governance logic."**
