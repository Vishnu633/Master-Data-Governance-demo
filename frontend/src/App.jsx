import React, { useState, useEffect } from 'react';
import './App.css';

const API_BASE = 'http://localhost:5181/api';

const safeParseResponse = async (res) => {
  const text = await res.text();
  try {
    return JSON.parse(text);
  } catch (err) {
    return { message: text };
  }
};

function App() {
  const [activeTab, setActiveTab] = useState('cataloging');
  const [templates, setTemplates] = useState([]);
  
  // Selection States
  const [selectedNoun, setSelectedNoun] = useState('');
  const [selectedModifier, setSelectedModifier] = useState('');
  const [activeTemplate, setActiveTemplate] = useState(null);
  const [attributeInputs, setAttributeInputs] = useState({});
  
  // Data Grid States
  const [stagingItems, setStagingItems] = useState([]);
  const [productionItems, setProductionItems] = useState([]);
  
  // Loading & UI feedback
  const [loading, setLoading] = useState({ templates: false, staging: false, production: false });
  const [feedback, setFeedback] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Fetch Templates on Mount
  useEffect(() => {
    fetchTemplates();
  }, []);

  // Fetch Staging or Production items when tabs switch
  useEffect(() => {
    if (activeTab === 'staging') {
      fetchStagingItems();
    } else if (activeTab === 'production') {
      fetchProductionItems();
    }
  }, [activeTab]);

  // Handle Noun/Modifier changes to load matching template
  useEffect(() => {
    if (selectedNoun && selectedModifier) {
      const match = templates.find(
        t => t.noun.toUpperCase() === selectedNoun.toUpperCase() && 
             t.modifier.toUpperCase() === selectedModifier.toUpperCase()
      );
      if (match) {
        setActiveTemplate(match);
        // Initialize attribute input fields with empty strings
        const initialInputs = {};
        match.requiredAttributes.forEach(attr => {
          initialInputs[attr] = '';
        });
        setAttributeInputs(initialInputs);
      } else {
        setActiveTemplate(null);
        setAttributeInputs({});
      }
    } else {
      setActiveTemplate(null);
      setAttributeInputs({});
    }
  }, [selectedNoun, selectedModifier, templates]);

  const fetchTemplates = async () => {
    setLoading(prev => ({ ...prev, templates: true }));
    try {
      const res = await fetch(`${API_BASE}/templates`);
      if (res.ok) {
        const data = await safeParseResponse(res);
        setTemplates(data);
      } else {
        showFeedback('danger', 'Failed to Load Templates', 'Error retrieving governance templates from backend.');
      }
    } catch (err) {
      showFeedback('danger', 'Connection Error', 'Could not connect to backend API server.');
    } finally {
      setLoading(prev => ({ ...prev, templates: false }));
    }
  };

  const fetchStagingItems = async () => {
    setLoading(prev => ({ ...prev, staging: true }));
    try {
      const res = await fetch(`${API_BASE}/staging`);
      if (res.ok) {
        const data = await safeParseResponse(res);
        setStagingItems(data);
      }
    } catch (err) {
      showFeedback('danger', 'Error', 'Failed to fetch staging requests.');
    } finally {
      setLoading(prev => ({ ...prev, staging: false }));
    }
  };

  const fetchProductionItems = async () => {
    setLoading(prev => ({ ...prev, production: true }));
    try {
      const res = await fetch(`${API_BASE}/production`);
      if (res.ok) {
        const data = await safeParseResponse(res);
        setProductionItems(data);
      }
    } catch (err) {
      showFeedback('danger', 'Error', 'Failed to fetch production catalog records.');
    } finally {
      setLoading(prev => ({ ...prev, production: false }));
    }
  };

  const showFeedback = (type, title, detail, errors = []) => {
    setFeedback({ type, title, detail, errors });
    // Auto clear success feedbacks after 8s
    if (type === 'success') {
      setTimeout(() => setFeedback(null), 8000);
    }
  };

  const handleAttributeChange = (attrName, value) => {
    setAttributeInputs(prev => ({
      ...prev,
      [attrName]: value
    }));
  };

  const resetForm = () => {
    setSelectedNoun('');
    setSelectedModifier('');
    setActiveTemplate(null);
    setAttributeInputs({});
  };

  // Submit Draft Material (Minimal validation)
  const handleSubmitDraft = async (e) => {
    e.preventDefault();
    if (!selectedNoun || !selectedModifier) return;

    setIsSubmitting(true);
    setFeedback(null);

    const payload = {
      noun: selectedNoun,
      modifier: selectedModifier,
      attributeValues: attributeInputs,
      status: 0 // Draft
    };

    try {
      const res = await fetch(`${API_BASE}/staging`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      const data = await safeParseResponse(res);
      if (res.ok) {
        showFeedback(
          'success', 
          'Draft Saved Successfully', 
          `Draft Staged ID: ${data.id}. Unique ID: ${data.uniqueId || 'N/A'}`
        );
        resetForm();
      } else {
        showFeedback(
          'danger', 
          'Failed to Save Draft', 
          data.message || 'Validation error saving draft.'
        );
      }
    } catch (err) {
      showFeedback('danger', 'Connection Error', 'Failed to reach API server.');
    } finally {
      setIsSubmitting(false);
    }
  };

  // Submit Governance Request (Strict validation & De-duplication check)
  const handleSubmitGovernance = async (e) => {
    e.preventDefault();
    if (!selectedNoun || !selectedModifier) return;

    setIsSubmitting(true);
    setFeedback(null);

    const payload = {
      noun: selectedNoun,
      modifier: selectedModifier,
      attributeValues: attributeInputs
    };

    try {
      const res = await fetch(`${API_BASE}/governance/request`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      const data = await safeParseResponse(res);
      if (res.ok) {
        showFeedback(
          'success', 
          'Governance Request Submitted', 
          `Material successfully validated, nomenclature generated, and staged as Pending. Unique ID: ${data.uniqueId}`
        );
        resetForm();
      } else {
        // Backend returns duplicate error message or validation errors
        const detail = typeof data === 'string' ? data : (data.message || 'Governance submission rejected.');
        const errorsList = data.errors || [];
        showFeedback('danger', 'Submission Blocked', detail, errorsList);
      }
    } catch (err) {
      showFeedback('danger', 'Connection Error', 'Failed to reach governance engine API.');
    } finally {
      setIsSubmitting(false);
    }
  };

  // Approve Pending Staging Item
  const handleApprove = async (id) => {
    setFeedback(null);
    try {
      const res = await fetch(`${API_BASE}/staging/${id}/approve`, {
        method: 'POST'
      });

      const data = await safeParseResponse(res);
      if (res.ok) {
        showFeedback(
          'success', 
          'Golden Record Promoted', 
          `Staging ID ${id} approved successfully and promoted to Production Catalog. Description: ${data.productionRecord.description}`
        );
        fetchStagingItems();
      } else {
        showFeedback(
          'danger', 
          'Approval Denied', 
          data.message || 'Error occurred during material promotion.'
        );
      }
    } catch (err) {
      showFeedback('danger', 'Connection Error', 'Failed to connect to backend.');
    }
  };

  // Helper to format attribute keys nicely
  const formatAttrLabel = (str) => {
    return str.replace(/_/g, ' ');
  };

  // Helper to get status badge class
  const getStatusBadgeClass = (status) => {
    switch (status) {
      case 0: return 'status-badge-draft';
      case 1: return 'status-badge-pending';
      case 2: return 'status-badge-approved';
      default: return '';
    }
  };

  const getStatusText = (status) => {
    switch (status) {
      case 0: return 'Draft';
      case 1: return 'Pending';
      case 2: return 'Approved';
      default: return 'Unknown';
    }
  };

  // Unique list of Nouns & Modifiers filtered by selected Noun
  const nounsList = [...new Set(templates.map(t => t.noun))];
  const modifiersList = selectedNoun
    ? [...new Set(templates.filter(t => t.noun.toUpperCase() === selectedNoun.toUpperCase()).map(t => t.modifier))]
    : [];

  return (
    <div className="dashboard-container">
      {/* Header */}
      <header className="dashboard-header">
        <div className="brand-section">
          <div className="logo-container">
            <img 
              src="https://hofinsoft.com/wp-content/uploads/2023/01/hofinsoft-new-logo.png" 
              alt="Hofinsoft Technologies" 
              className="brand-logo"
            />
          </div>
          <p className="brand-tagline">eNOMCAT — Gated Material Master Governance Portal</p>
        </div>
        <div className="system-status-pill">
          <span className="status-dot"></span>
          <span>MDG Core Active</span>
        </div>
      </header>

      {/* Tabs */}
      <nav className="tabs-navigation">
        <button 
          className={`tab-btn ${activeTab === 'cataloging' ? 'active' : ''}`}
          onClick={() => setActiveTab('cataloging')}
        >
          ✍️ Item Cataloging
        </button>
        <button 
          className={`tab-btn ${activeTab === 'staging' ? 'active' : ''}`}
          onClick={() => setActiveTab('staging')}
        >
          📋 Staging Board
        </button>
        <button 
          className={`tab-btn ${activeTab === 'production' ? 'active' : ''}`}
          onClick={() => setActiveTab('production')}
        >
          🏆 Golden Records
        </button>
      </nav>

      {/* Notification Feedback Banners */}
      {feedback && (
        <div className={`alert-banner alert-banner-${feedback.type}`}>
          <div>
            <div className="alert-title">{feedback.title}</div>
            <div className="alert-detail">{feedback.detail}</div>
            {feedback.errors && feedback.errors.length > 0 && (
              <ul className="alert-list">
                {feedback.errors.map((err, idx) => (
                  <li key={idx}>{err}</li>
                ))}
              </ul>
            )}
          </div>
        </div>
      )}

      {/* Tab Contents */}
      {activeTab === 'cataloging' && (
        <div className="dashboard-grid">
          {/* Item Creation Form */}
          <div className="glass-panel">
            <h2 className="panel-title">Item Cataloging Form</h2>
            <form onSubmit={e => e.preventDefault()}>
              <div className="form-group">
                <label className="form-label">Noun</label>
                <select 
                  className="form-select"
                  value={selectedNoun}
                  onChange={e => setSelectedNoun(e.target.value)}
                >
                  <option value="">-- Choose Noun --</option>
                  {nounsList.map((noun, idx) => (
                    <option key={idx} value={noun}>{noun}</option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label className="form-label">Modifier</label>
                <select 
                  className="form-select"
                  value={selectedModifier}
                  onChange={e => setSelectedModifier(e.target.value)}
                  disabled={!selectedNoun}
                >
                  <option value="">-- Choose Modifier --</option>
                  {modifiersList.map((mod, idx) => (
                    <option key={idx} value={mod}>{mod}</option>
                  ))}
                </select>
              </div>

              {/* Dynamic Field Generator */}
              {activeTemplate && (
                <div className="dynamic-fields-section">
                  <h3 className="dynamic-fields-title">
                    🔧 Dynamic Schema: {activeTemplate.noun} {activeTemplate.modifier}
                  </h3>
                  {activeTemplate.requiredAttributes.map((attr, idx) => (
                    <div className="form-group" key={idx}>
                      <label className="form-label">{formatAttrLabel(attr)}</label>
                      <input 
                        type="text"
                        className="form-input"
                        placeholder={`Enter ${formatAttrLabel(attr)}...`}
                        value={attributeInputs[attr] || ''}
                        onChange={e => handleAttributeChange(attr, e.target.value)}
                      />
                    </div>
                  ))}
                </div>
              )}

              {/* Buttons */}
              <div className="btn-group">
                <button 
                  type="button" 
                  className={`btn btn-secondary ${isSubmitting || !activeTemplate ? 'btn-disabled' : ''}`}
                  onClick={handleSubmitDraft}
                  disabled={isSubmitting || !activeTemplate}
                >
                  📁 Save as Draft
                </button>
                <button 
                  type="submit" 
                  className={`btn btn-primary ${isSubmitting || !activeTemplate ? 'btn-disabled' : ''}`}
                  onClick={handleSubmitGovernance}
                  disabled={isSubmitting || !activeTemplate}
                >
                  🚀 Submit Governance
                </button>
              </div>
            </form>
          </div>

          {/* Guide Panel */}
          <div className="glass-panel">
            <h2 className="panel-title">Governance Info</h2>
            <div className="info-card">
              <div className="info-row">
                <span className="info-label">Active Schema Profiles</span>
                <span className="info-value">{templates.length}</span>
              </div>
              <div className="info-row">
                <span className="info-label">Sample Profile (Seeded)</span>
                <span className="info-value code-badge">BEARING BALL</span>
              </div>
            </div>
            <div style={{ fontSize: '0.9rem', color: 'var(--text-secondary)', lineHeight: '1.6' }}>
              <p style={{ marginBottom: '0.75rem' }}>
                💡 <strong>Dynamic Field Generator:</strong> Selecting <strong>Noun: BEARING</strong> and <strong>Modifier: BALL</strong> fetches metadata from the SQLite data dictionary and dynamically displays input fields for the required attributes.
              </p>
              <p style={{ marginBottom: '0.75rem' }}>
                📁 <strong>Draft submissions</strong> skip strict attribute validation, allowing catalogers to save partial details for future editing.
              </p>
              <p>
                🚀 <strong>Governance submissions</strong> enforce full schema compliance, generate standardized descriptions, and block duplicate entries.
              </p>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'staging' && (
        <div className="glass-panel">
          <h2 className="panel-title">Staging Item Requests Board</h2>
          {loading.staging ? (
            <div className="empty-state">Loading staging requests...</div>
          ) : stagingItems.length === 0 ? (
            <div className="empty-state">
              <h3>No items in staging</h3>
              <p>Go to the Item Cataloging tab to submit drafts or governance requests.</p>
            </div>
          ) : (
            <div className="grid-container">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Unique ID</th>
                    <th>Noun</th>
                    <th>Modifier</th>
                    <th>Attribute Values</th>
                    <th>Nomenclature Description</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {stagingItems.map((item) => (
                    <tr key={item.id}>
                      <td>{item.id}</td>
                      <td>
                        <span className="code-badge">{item.uniqueId || 'Generating...'}</span>
                      </td>
                      <td style={{ fontWeight: '600' }}>{item.noun}</td>
                      <td>{item.modifier}</td>
                      <td>
                        <div className="attributes-tags">
                          {Object.entries(item.attributeValues).map(([key, val]) => (
                            <span key={key} className="attr-tag">
                              <strong>{formatAttrLabel(key)}:</strong> {val || 'n/a'}
                            </span>
                          ))}
                        </div>
                      </td>
                      <td style={{ fontStyle: 'italic', fontSize: '0.85rem' }}>
                        {item.description || 'Draft (No nomenclature generated)'}
                      </td>
                      <td>
                        <span className={`status-badge ${getStatusBadgeClass(item.status)}`}>
                          {getStatusText(item.status)}
                        </span>
                      </td>
                      <td>
                        {item.status === 1 ? (
                          <button 
                            className="btn btn-success"
                            onClick={() => handleApprove(item.id)}
                          >
                            ✔️ Approve
                          </button>
                        ) : (
                          <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                            {item.status === 2 ? 'Promoted' : 'Draft'}
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {activeTab === 'production' && (
        <div className="glass-panel">
          <h2 className="panel-title">Production Golden Records Catalog</h2>
          {loading.production ? (
            <div className="empty-state">Loading golden records...</div>
          ) : productionItems.length === 0 ? (
            <div className="empty-state">
              <h3>No production golden records</h3>
              <p>Approve pending staging items on the Staging Board to promote them here.</p>
            </div>
          ) : (
            <div className="grid-container">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Unique ID</th>
                    <th>Noun</th>
                    <th>Modifier</th>
                    <th>Attribute Specifications</th>
                    <th>Standardized Nomenclature</th>
                    <th>Approved At</th>
                  </tr>
                </thead>
                <tbody>
                  {productionItems.map((item) => (
                    <tr key={item.id}>
                      <td>{item.id}</td>
                      <td>
                        <span className="code-badge" style={{ borderColor: 'var(--status-approved)', color: '#34d399' }}>
                          {item.uniqueId}
                        </span>
                      </td>
                      <td style={{ fontWeight: '600' }}>{item.noun}</td>
                      <td>{item.modifier}</td>
                      <td>
                        <div className="attributes-tags">
                          {Object.entries(item.attributeValues).map(([key, val]) => (
                            <span key={key} className="attr-tag" style={{ background: 'rgba(52, 211, 153, 0.05)' }}>
                              <strong>{formatAttrLabel(key)}:</strong> {val}
                            </span>
                          ))}
                        </div>
                      </td>
                      <td style={{ fontStyle: 'italic', color: '#e2e8f0', fontSize: '0.85rem' }}>
                        {item.description}
                      </td>
                      <td style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
                        {new Date(item.approvedAt).toLocaleString()}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default App;
