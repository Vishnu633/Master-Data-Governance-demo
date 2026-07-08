import { useState, useEffect, useCallback, useRef } from 'react';
import './App.css';

const API = 'http://localhost:5181/api';

// ─── Clean Vector SVG Icons ────────────────────────
const DashboardIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><rect x="3" y="3" width="7" height="9"/><rect x="14" y="3" width="7" height="5"/><rect x="14" y="12" width="7" height="9"/><rect x="3" y="16" width="7" height="5"/></svg>
);
const CatalogIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/></svg>
);
const WorkTrayIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
);
const NewRequestIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><path d="M12 5v14M5 12h14"/></svg>
);
const BulkImportIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4M17 8l-5-5-5 5M12 3v12"/></svg>
);
const ReportingIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/></svg>
);
const SearchIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
);
const BellIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9M13.73 21a2 2 0 0 1-3.46 0"/></svg>
);
const HelpIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><circle cx="12" cy="12" r="10"/><path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3M12 17h.01"/></svg>
);
const RobotIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><rect x="3" y="11" width="18" height="10" rx="2"/><circle cx="12" cy="5" r="2"/><path d="M12 7v4M8 16h.01M16 16h.01"/></svg>
);
const CloseIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
);
const SuccessIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
);
const WarningIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0zM12 9v4M12 17h.01"/></svg>
);
const ChatIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
);

// ─── Preconfigured Simulated Users ──────────────────────────
const SIMULATED_USERS = [
  { id: 1, name: 'Sarah J.', role: 'Requester', email: 'sarah.j@nomcat.io', avatarColor: '#3b82f6', tag: 'R', desc: 'Can submit new requests & bulk uploads' },
  { id: 2, name: 'Michael R.', role: 'Approver', email: 'michael.r@nomcat.io', avatarColor: '#f59e0b', tag: 'A', desc: 'First-level approval & staging supervisor' },
  { id: 3, name: 'Alex C.', role: 'CentralCataloger', email: 'alex.c@nomcat.io', avatarColor: '#10b981', tag: 'C', desc: 'Standardizes nomenclature and specifications' },
  { id: 4, name: 'Sophia K.', role: 'CentralApprover', email: 'sophia.k@nomcat.io', avatarColor: '#8b5cf6', tag: 'CA', desc: 'Final sign-off to publish to Golden Catalog' }
];

// ─── Zero-Dependency Markdown Parser ────────────────────────
const renderMarkdown = (text) => {
  if (!text) return "";
  const lines = text.split('\n');
  return lines.map((line, idx) => {
    let isBullet = false;
    let isHeader = false;
    let headerLevel = 0;
    let cleanLine = line;

    if (line.trim().startsWith('- ') || line.trim().startsWith('* ') || line.trim().startsWith('• ')) {
      isBullet = true;
      cleanLine = line.trim().substring(2);
    } else if (line.trim().startsWith('### ')) {
      isHeader = true;
      headerLevel = 3;
      cleanLine = line.trim().substring(4);
    } else if (line.trim().startsWith('## ')) {
      isHeader = true;
      headerLevel = 2;
      cleanLine = line.trim().substring(3);
    } else if (line.trim().startsWith('# ')) {
      isHeader = true;
      headerLevel = 1;
      cleanLine = line.trim().substring(2);
    }
    
    const parts = [];
    const regex = /(\*\*.*?\*\*|\*.*?\*|`.*?`)/g;
    const splitParts = cleanLine.split(regex);
    
    splitParts.forEach((part, pIdx) => {
      if (part.startsWith('**') && part.endsWith('**')) {
        parts.push(<strong key={pIdx} style={{ color: '#fff', fontWeight: '700' }}>{part.slice(2, -2)}</strong>);
      } else if (part.startsWith('*') && part.endsWith('*')) {
        parts.push(<em key={pIdx}>{part.slice(1, -1)}</em>);
      } else if (part.startsWith('`') && part.endsWith('`')) {
        parts.push(<code key={pIdx} style={{ background: 'rgba(255,255,255,0.1)', padding: '2px 4px', borderRadius: '3px', fontFamily: 'monospace', color: '#a5b4fc', fontSize: '0.72rem' }}>{part.slice(1, -1)}</code>);
      } else {
        parts.push(part);
      }
    });

    if (isBullet) {
      return (
        <li key={idx} style={{ marginLeft: '1rem', listStyleType: 'disc', marginTop: '3px', marginBottom: '3px' }}>
          {parts}
        </li>
      );
    }

    if (isHeader) {
      const fontSize = headerLevel === 1 ? '0.85rem' : headerLevel === 2 ? '0.78rem' : '0.74rem';
      return (
        <div key={idx} style={{ margin: '0.6rem 0 0.3rem 0', fontWeight: '700', color: '#fff', fontSize: fontSize, borderBottom: headerLevel === 1 ? '1px solid rgba(255,255,255,0.1)' : 'none', paddingBottom: headerLevel === 1 ? '0.2rem' : '0' }}>
          {parts}
        </div>
      );
    }

    return <div key={idx} style={{ minHeight: '1.1em' }}>{parts}</div>;
  });
};

// ─── Toast Notification System ───────────────────────────────
function ToastContainer({ toasts, onDismiss }) {
  return (
    <div className="toast-container">
      {toasts.map((t) => (
        <div key={t.id} className={`toast ${t.type}`}>
          <span className="toast-icon">
            {t.type === 'success' ? '✅' : t.type === 'error' ? '❌' : '⚠️'}
          </span>
          <div style={{ flex: 1 }}>
            <div style={{ fontWeight: 600, fontSize: '0.8rem', color: '#fff' }}>{t.title}</div>
            <div style={{ fontSize: '0.72rem', color: 'var(--text-secondary)', marginTop: '2px' }}>{t.message}</div>
          </div>
          <button className="toast-close" onClick={() => onDismiss(t.id)}>×</button>
        </div>
      ))}
    </div>
  );
}

function useToast() {
  const [toasts, setToasts] = useState([]);
  const addToast = useCallback((type, title, message) => {
    const id = Date.now() + Math.random();
    setToasts((prev) => [...prev, { id, type, title, message }]);
    setTimeout(() => setToasts((prev) => prev.filter((t) => t.id !== id)), 5000);
  }, []);
  const dismissToast = useCallback((id) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);
  return { toasts, addToast, dismissToast };
}

// ─── Status Badge ────────────────────────────────────────────
function StatusBadge({ status }) {
  const classMap = {
    Stage1_Validated: 'badge-validated',
    In_Progress: 'badge-progress',
    Approved: 'badge-approved',
    DUPLICATED: 'badge-duplicated',
    Duplicated: 'badge-duplicated',
    Rejected: 'badge-rejected',
    Pending: 'badge-pending',
  };
  return <span className={`badge ${classMap[status] || 'badge-pending'}`}>{status?.replace(/_/g, ' ')}</span>;
}

// ─── Pipeline Visual ─────────────────────────────────────────
function PipelineVisual({ currentStage, totalStages }) {
  const dots = [];
  for (let i = 1; i <= totalStages; i++) {
    if (i > 1) {
      dots.push(
        <div key={`line-${i}`} className={`pipeline-line ${i <= currentStage ? 'completed' : ''}`} />
      );
    }
    dots.push(
      <div
        key={`dot-${i}`}
        className={`pipeline-dot ${i < currentStage ? 'completed' : i === currentStage ? 'current' : ''}`}
        title={`Stage ${i}`}
      />
    );
  }
  return <div className="pipeline-visual">{dots}</div>;
}

// ─── Login Screen ────────────────────────────────────────────
function Login({ onLogin }) {
  const [selectedUser, setSelectedUser] = useState(null);

  const handleSignIn = (e) => {
    e.preventDefault();
    if (selectedUser) {
      onLogin(selectedUser);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="login-header">
          <div className="login-logo">N</div>
          <h2 className="login-title">NOMCAT</h2>
          <span className="login-subtitle">Enterprise MDG Platform</span>
        </div>

        <form onSubmit={handleSignIn} className="login-form">
          <label className="form-label" style={{ marginBottom: '-0.25rem' }}>Select Simulated Identity Role:</label>
          <div className="login-role-grid">
            {SIMULATED_USERS.map((user) => (
              <div
                key={user.id}
                className={`login-role-card ${selectedUser?.id === user.id ? 'selected' : ''}`}
                onClick={() => setSelectedUser(user)}
              >
                <span className="role-card-icon">{user.tag}</span>
                <span className="role-card-title">{user.name}</span>
                <span className="role-card-desc">{user.role.replace(/([A-Z])/g, ' $1').trim()}</span>
              </div>
            ))}
          </div>

          {selectedUser && (
            <div style={{ background: 'rgba(255,255,255,0.02)', border: '1px solid var(--border-primary)', padding: '0.75rem', borderRadius: '4px', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
              <div><strong>Email:</strong> {selectedUser.email}</div>
              <div style={{ marginTop: '2px' }}><strong>Access Scope:</strong> {selectedUser.desc}</div>
            </div>
          )}

          <button type="submit" className="btn btn-primary" style={{ width: '100%', padding: '0.7rem' }} disabled={!selectedUser}>
            Sign In to MDG Workspace
          </button>
        </form>
      </div>
    </div>
  );
}

// ─── Approval Timeline Component ─────────────────────────────
function ApprovalTimeline({ request }) {
  if (!request) return null;
  
  let logData = [];
  try {
    logData = JSON.parse(request.approvalLog || "[]");
  } catch (e) {
    console.error("Error parsing approval log", e);
  }

  const isExtension = request.requestType === 'Plant_Extension';
  const stagesList = isExtension 
    ? [
        { stage: 1, role: 'Requester', title: 'Requester Submission', desc: 'Material request registered' },
        { stage: 2, role: 'Approver', title: 'Local Approver Review', desc: 'Plant-level review' },
        { stage: 3, role: 'CentralCataloger', title: 'Central Cataloger Standardization', desc: 'Final classification review & catalog promotion' }
      ]
    : [
        { stage: 1, role: 'Requester', title: 'Requester Submission', desc: 'Material request registered' },
        { stage: 2, role: 'Approver', title: 'Local Approver Review', desc: 'Plant-level review' },
        { stage: 3, role: 'CentralCataloger', title: 'Central Cataloger Review', desc: 'Standardizes nomenclature and attribute values' },
        { stage: 4, role: 'CentralApprover', title: 'Central Approver Release', desc: 'Final authorization & promotion to Golden Master' }
      ];

  return (
    <div className="timeline-container">
      {stagesList.map((stg) => {
        const logEntry = logData.find(entry => entry.Stage === stg.stage || entry.Role === stg.role);
        
        let statusClass = '';
        let statusTitle = stg.title;
        let timeString = '';
        let comment = '';
        let userName = '';

        if (logEntry) {
          statusClass = 'completed';
          userName = logEntry.User || logEntry.ApprovedBy || 'System';
          timeString = logEntry.Timestamp 
            ? new Date(logEntry.Timestamp).toLocaleString() 
            : new Date(request.createdAt).toLocaleString();
          if (logEntry.Comment) comment = logEntry.Comment;
          statusTitle += ` (${logEntry.Action || 'Approved'})`;
        } else if (stg.stage === request.currentStage && request.approvalStatus !== 'Approved' && request.approvalStatus !== 'Rejected') {
          statusClass = 'active';
          statusTitle += ' — Active';
          userName = `Pending with: ${stg.role === 'CentralCataloger' ? 'Cataloger' : stg.role}`;
        } else {
          userName = `Pending Stage ${stg.stage}`;
        }

        if (request.approvalStatus === 'Approved') {
          statusClass = 'completed';
          if (stg.stage === request.totalStages) {
            timeString = new Date(request.updatedAt).toLocaleString();
            userName = request.modifier || 'Central System';
          }
        }
        
        if (request.approvalStatus === 'Rejected' && stg.stage === request.currentStage) {
          statusClass = 'rejected';
          statusTitle += ' — Rejected';
        }

        return (
          <div className="timeline-item" key={stg.stage}>
            <div className={`timeline-badge ${statusClass}`} />
            <div className="timeline-content">
              <span className="timeline-title">{statusTitle}</span>
              <span className="timeline-meta">
                {userName} {timeString && `• ${timeString}`}
              </span>
              {comment && <div className="timeline-comment">💬 "{comment}"</div>}
            </div>
          </div>
        );
      })}
    </div>
  );
}

// ─── Main App ────────────────────────────────────────────────
function App() {
  const [currentUser, setCurrentUser] = useState(() => {
    const saved = localStorage.getItem('nomcat_session');
    return saved ? JSON.parse(saved) : null;
  });

  const [activeTab, setActiveTab] = useState('dashboard');
  const [profiles, setProfiles] = useState([]);
  const [summary, setSummary] = useState({});
  const [requests, setRequests] = useState([]);
  const [catalog, setCatalog] = useState([]);
  const { toasts, addToast: originalAddToast, dismissToast } = useToast();
  const [notifications, setNotifications] = useState([
    { id: 1, type: 'success', title: 'System Initialized', message: 'NomCat Master Data Governance console active.', timestamp: new Date(), read: false }
  ]);
  const [showNotificationsDropdown, setShowNotificationsDropdown] = useState(false);
  const [showHelpModal, setShowHelpModal] = useState(false);
  const [dashboardSubTab, setDashboardSubTab] = useState('operations'); // 'operations' or 'insights'

  const addToast = useCallback((type, title, message) => {
    originalAddToast(type, title, message);
    setNotifications(prev => [
      {
        id: Date.now() + Math.random(),
        type,
        title,
        message,
        timestamp: new Date(),
        read: false
      },
      ...prev
    ]);
  }, [originalAddToast]);

  // NomBot Chatbot State
  const [isBotOpen, setIsBotOpen] = useState(false);
  const [chatMessage, setChatMessage] = useState('');
  const [chatLogs, setChatLogs] = useState([
    { sender: 'bot', text: "👋 Hello! I'm **NomBot**, your Master Data Governance cataloging assistant. Ask me anything about our requests, golden records, or plant extensions." }
  ]);
  const [askingBot, setAskingBot] = useState(false);

  // New Request Form state
  const [selectedProfile, setSelectedProfile] = useState('');
  const [requestType, setRequestType] = useState('Single');
  const [plant, setPlant] = useState('PLT1');
  const [priority, setPriority] = useState('Standard');
  const [expectedDate, setExpectedDate] = useState('2024-05-10');
  const [schema, setSchema] = useState(null);
  const [attrValues, setAttrValues] = useState({});
  const [submitting, setSubmitting] = useState(false);

  // Plant Extension - Search Golden Catalog
  const [searchQuery, setSearchQuery] = useState('');
  const [showGoldenDropdown, setShowGoldenDropdown] = useState(false);
  const [selectedGoldenRecord, setSelectedGoldenRecord] = useState(null);
  const [globalSearch, setGlobalSearch] = useState('');

  // Bulk upload state
  const [bulkResults, setBulkResults] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [dragOver, setDragOver] = useState(false);
  const fileInputRef = useRef(null);

  // User dropdown menu
  const [showProfileDropdown, setShowProfileDropdown] = useState(false);

  // Filters for Work Tray table view
  const [wtSearch, setWtSearch] = useState('');
  const [wtStatus, setWtStatus] = useState('');
  const [wtPlant, setWtPlant] = useState('');
  const [selectedRequest, setSelectedRequest] = useState(null);

  // AI Assist state
  const [aiDescription, setAiDescription] = useState('');
  const [aiClassifying, setAiClassifying] = useState(false);
  const [aiResult, setAiResult] = useState(null);
  const [similarityWarning, setSimilarityWarning] = useState(null);

  // AI Multimodal & Audit states
  const [imageFile, setImageFile] = useState(null);
  const [imageClassifying, setImageClassifying] = useState(false);
  const [imagePreview, setImagePreview] = useState(null);
  const [auditReport, setAuditReport] = useState(null);
  const [auditing, setAuditing] = useState(false);

  // AI Bulk Cleansing states
  const [rawDescriptions, setRawDescriptions] = useState('');
  const [cleansing, setCleansing] = useState(false);
  const [cleansedPreview, setCleansedPreview] = useState(null);

  // Real-time similarity check debounced effect
  useEffect(() => {
    if (!selectedProfile || Object.keys(attrValues).length === 0 || requestType === 'Plant_Extension') {
      setSimilarityWarning(null);
      return;
    }

    const filledValues = Object.values(attrValues).filter(v => typeof v === 'string' && v.trim() !== '');
    if (filledValues.length === 0) {
      setSimilarityWarning(null);
      return;
    }

    const timer = setTimeout(() => {
      const [noun, modifier] = selectedProfile.split('|');
      const queryStr = `${noun} ${modifier} ${Object.entries(attrValues)
        .map(([k, v]) => `${k}:${v}`)
        .join(' ')}`;
      handleAiSearch(queryStr);
    }, 1200);

    return () => clearTimeout(timer);
  }, [attrValues, selectedProfile, requestType]);

  // ─── AI Classification Handler ────────────────────────────
  const handleAiClassify = async () => {
    if (!aiDescription.trim()) return;
    setAiClassifying(true);
    setAiResult(null);
    setSimilarityWarning(null);
    try {
      const res = await fetch(`${API}/ai/classify`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ description: aiDescription })
      });
      if (res.status === 503) {
        addToast('warning', 'AI Not Configured', 'Set GEMINI_API_KEY on the server to enable AI classification.');
        setAiClassifying(false);
        return;
      }
      if (!res.ok) {
        addToast('error', 'Classification Failed', 'AI could not extract structured data from your description.');
        setAiClassifying(false);
        return;
      }
      const data = await res.json();
      setAiResult(data);

      const profileKey = `${data.noun}|${data.modifier}`;
      const matchingProfile = profiles.find(p => `${p.noun}|${p.modifier}` === profileKey);
      if (matchingProfile) {
        handleProfileChange(profileKey);
        if (data.plant) setPlant(data.plant.toUpperCase());
        setTimeout(() => {
          if (data.attributes) {
            setAttrValues(prev => ({ ...prev, ...data.attributes }));
          }
        }, 500);
        addToast('success', 'AI Classification Complete', `Matched to ${data.noun}/${data.modifier} (${Math.round(data.confidence * 100)}% confidence)`);
      } else {
        addToast('warning', 'Profile Not Found', `AI suggested ${data.noun}/${data.modifier} but no matching profile exists in the system.`);
      }
    } catch (err) {
      addToast('error', 'AI Error', err.message);
    }
    setAiClassifying(false);
  };

  // ─── AI Image Classification Handler ───────────────────────
  const handleImageClassify = async () => {
    if (!imageFile) return;
    setImageClassifying(true);
    setAiResult(null);
    setSimilarityWarning(null);

    const formData = new FormData();
    formData.append('file', imageFile);

    try {
      const res = await fetch(`${API}/ai/classify-image`, {
        method: 'POST',
        body: formData
      });

      if (res.status === 503) {
        addToast('warning', 'AI Not Configured', 'Set GEMINI_API_KEY to enable AI image classification.');
        setImageClassifying(false);
        return;
      }
      if (!res.ok) {
        addToast('error', 'Classification Failed', 'AI could not classify the image of this part.');
        setImageClassifying(false);
        return;
      }

      const data = await res.json();
      setAiResult(data);

      const profileKey = `${data.noun}|${data.modifier}`;
      const matchingProfile = profiles.find(p => `${p.noun}|${p.modifier}` === profileKey);
      if (matchingProfile) {
        handleProfileChange(profileKey);
        if (data.plant) setPlant(data.plant.toUpperCase());
        setTimeout(() => {
          if (data.attributes) {
            setAttrValues(prev => ({ ...prev, ...data.attributes }));
          }
        }, 500);
        addToast('success', 'Image Classification Complete', `Matched to ${data.noun}/${data.modifier}`);
      } else {
        addToast('warning', 'Profile Not Found', `AI suggested ${data.noun}/${data.modifier} but no matching profile exists.`);
      }
    } catch (err) {
      addToast('error', 'AI Image Error', err.message);
    }
    setImageClassifying(false);
  };

  // ─── AI Data Auditor Steward Handler ───────────────────────
  const handleAiAudit = async () => {
    if (!selectedProfile) return;
    setAuditing(true);
    setAuditReport(null);

    const [noun, modifier] = selectedProfile.split('|');
    try {
      const res = await fetch(`${API}/ai/audit`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          noun,
          modifier,
          attributes: attrValues
        })
      });

      if (res.ok) {
        const data = await res.json();
        setAuditReport(data.auditReport);
        addToast('success', 'AI Audit Complete', 'Governance specifications verified.');
      } else {
        addToast('error', 'Audit Failed', 'Could not run AI audit.');
      }
    } catch (e) {
      addToast('error', 'AI Audit Error', e.message);
    }
    setAuditing(false);
  };

  // ─── AI Bulk Cleansing Handlers ────────────────────────────
  const handleBulkClean = async () => {
    if (!rawDescriptions.trim()) return;
    setCleansing(true);
    setCleansedPreview(null);

    const descriptions = rawDescriptions.split('\n').map(d => d.trim()).filter(d => d.length > 0);
    try {
      const res = await fetch(`${API}/ai/bulk-clean`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ descriptions })
      });

      if (res.ok) {
        const data = await res.json();
        setCleansedPreview(data);
        addToast('success', 'AI Cleansing Complete', `Successfully cleansed and parsed ${data.length} materials.`);
      } else {
        addToast('error', 'Cleansing Failed', 'AI could not clean the legacy descriptions.');
      }
    } catch (e) {
      addToast('error', 'AI Cleansing Error', e.message);
    }
    setCleansing(false);
  };

  const handleSubmitCleansed = async () => {
    if (!cleansedPreview || cleansedPreview.length === 0) return;
    setUploading(true);

    try {
      const items = cleansedPreview.map(item => ({
        noun: item.noun,
        modifier: item.modifier,
        requestType: 'Single',
        plant: item.plant || 'PLT1',
        attributes: item.attributes
      }));

      const res = await fetch(`${API}/requests/bulk`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ items })
      });

      const data = await res.json();
      setBulkResults(data);
      setCleansedPreview(null);
      setRawDescriptions('');

      if (data.successful > 0) {
        addToast('success', 'Staging Ingested', `Successfully imported ${data.successful} cleansed materials to staging.`);
      } else {
        addToast('warning', 'Bulk Import Result', `Duplicates: ${data.duplicates}, Errors: ${data.errors}`);
      }
      refreshAll();
    } catch (e) {
      addToast('error', 'Submission Error', e.message);
    } finally {
      setUploading(false);
    }
  };

  // ─── AI Semantic Search Handler ────────────────────────────
  const handleAiSearch = async (query) => {
    if (!query || query.length < 3) return;
    try {
      const res = await fetch(`${API}/ai/search`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ query })
      });
      if (res.ok) {
        const data = await res.json();
        if (data && data.length > 0) {
          setSimilarityWarning(data);
        } else {
          setSimilarityWarning(null);
        }
      }
    } catch (err) {
      console.error('Semantic search error:', err);
    }
  };

  // ─── Authentication Handlers ─────────────────────────────
  const handleLogin = (user) => {
    setCurrentUser(user);
    localStorage.setItem('nomcat_session', JSON.stringify(user));
    addToast('success', 'Workspace Authenticated', `Logged in as ${user.name} (${user.role})`);
  };

  const handleLogout = () => {
    setCurrentUser(null);
    localStorage.removeItem('nomcat_session');
    setShowProfileDropdown(false);
  };

  // ─── API Calls ───────────────────────────────────────────
  const fetchProfiles = useCallback(async () => {
    if (!currentUser) return;
    try {
      const res = await fetch(`${API}/attributes/profiles`);
      if (res.ok) setProfiles(await res.json());
    } catch (e) { console.error('Failed to fetch profiles:', e); }
  }, [currentUser]);

  const fetchSummary = useCallback(async () => {
    if (!currentUser) return;
    try {
      const res = await fetch(`${API}/reporting/summary`);
      if (res.ok) setSummary(await res.json());
    } catch (e) { console.error('Failed to fetch summary:', e); }
  }, [currentUser]);

  const fetchRequests = useCallback(async () => {
    if (!currentUser) return;
    try {
      const res = await fetch(`${API}/requests`);
      if (res.ok) setRequests(await res.json());
    } catch (e) { console.error('Failed to fetch requests:', e); }
  }, [currentUser]);

  const fetchCatalog = useCallback(async () => {
    if (!currentUser) return;
    try {
      const res = await fetch(`${API}/catalog`);
      if (res.ok) setCatalog(await res.json());
    } catch (e) { console.error('Failed to fetch catalog:', e); }
  }, [currentUser]);

  const refreshAll = useCallback(() => {
    fetchProfiles();
    fetchSummary();
    fetchRequests();
    fetchCatalog();
  }, [fetchProfiles, fetchSummary, fetchRequests, fetchCatalog]);

  useEffect(() => { refreshAll(); }, [refreshAll]);

  // ─── Profile/Schema Selection ────────────────────────────
  const handleProfileChange = async (profileKey) => {
    setSelectedProfile(profileKey);
    setSchema(null);
    setAttrValues({});

    if (!profileKey) return;
    const [noun, modifier] = profileKey.split('|');
    try {
      const res = await fetch(`${API}/attributes/schema?noun=${noun}&modifier=${modifier}`);
      if (res.ok) {
        const data = await res.json();
        setSchema(data);
        const initVals = {};
        data.fields.forEach((f) => { initVals[f.attributeName] = ''; });
        setAttrValues(initVals);
      }
    } catch (e) { console.error('Failed to fetch schema:', e); }
  };

  // ─── Plant Extension Golden Selection ─────────────────────
  const handleSelectGoldenRecord = async (record) => {
    setSelectedGoldenRecord(record);
    setSearchQuery(`${record.materialNumber} - ${record.shortDescription}`);
    setShowGoldenDropdown(false);
    
    // Load schema for this noun/modifier
    const res = await fetch(`${API}/attributes/schema?noun=${record.noun}&modifier=${record.modifier}`);
    if (res.ok) {
      const data = await res.json();
      setSchema(data);
      // Pre-fill attributes from Golden Record
      const values = JSON.parse(record.jsonAttributeValues);
      setAttrValues(values);
      setSelectedProfile(`${record.noun}|${record.modifier}`);
    }
  };

  // ─── Submit Request ──────────────────────────────────────
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!schema) return;

    let noun, modifier;
    if (requestType === 'Plant_Extension') {
      if (!selectedGoldenRecord) {
        addToast('warning', 'Validation Warning', 'Please select an existing Golden Record to extend.');
        return;
      }
      noun = selectedGoldenRecord.noun;
      modifier = selectedGoldenRecord.modifier;
    } else {
      const parts = selectedProfile.split('|');
      noun = parts[0];
      modifier = parts[1];
    }

    setSubmitting(true);

    try {
      const res = await fetch(`${API}/requests/submit`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          noun, modifier, requestType, plant,
          attributes: attrValues,
        }),
      });

      let data;
      const contentType = res.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        data = await res.json();
      } else {
        const text = await res.text();
        data = { message: text };
      }

      if (res.ok) {
        addToast('success', 'Request Created', `${data.requestRefNo} — ${data.shortDescription}`);
        // Reset
        setAttrValues({});
        setSelectedProfile('');
        setSchema(null);
        setSelectedGoldenRecord(null);
        setSearchQuery('');
        refreshAll();
        setActiveTab('workTray');
      } else if (res.status === 409) {
        addToast('warning', 'Duplication Blocked', data.message);
      } else {
        addToast('error', 'Submission Failed', typeof data === 'string' ? data : (data.message || data.title || JSON.stringify(data)));
      }
    } catch (e) {
      addToast('error', 'Network Error', e.message);
    } finally {
      setSubmitting(false);
    }
  };

  // ─── Approve/Reject ──────────────────────────────────────
  const handleApproval = async (requestId, action) => {
    try {
      const res = await fetch(`${API}/requests/approve`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ requestId, action, role: currentUser.role }),
      });

      let data;
      const contentType = res.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        data = await res.json();
      } else {
        const text = await res.text();
        data = { message: text };
      }

      if (res.ok) {
        addToast('success',
          action === 'Approve' ? 'Approved' : 'Rejected',
          data.message || `Request ${action.toLowerCase()}d successfully.`
        );
        refreshAll();
      } else {
        addToast('error', 'Action Failed', typeof data === 'string' ? data : (data.message || data.title || JSON.stringify(data)));
      }
    } catch (e) {
      addToast('error', 'Network Error', e.message);
    }
  };

  // ─── Delete Actions (Live Chart Update Helpers) ──────────
  const deleteStagingRequest = async (id) => {
    if (!window.confirm("Are you sure you want to delete this staging request?")) return;
    try {
      const res = await fetch(`${API}/requests/${id}`, { method: 'DELETE' });
      if (!res.ok) throw new Error(await res.text());
      addToast('success', 'Staging Queue', 'Staging request deleted.');
      if (selectedRequest?.id === id) {
        setSelectedRequest(null);
      }
      refreshAll();
    } catch (err) {
      addToast('error', 'Delete Failed', err.message);
    }
  };

  const deleteGoldenRecord = async (id) => {
    if (!window.confirm("Are you sure you want to delete this Golden Master Record?")) return;
    try {
      const res = await fetch(`${API}/catalog/${id}`, { method: 'DELETE' });
      if (!res.ok) throw new Error(await res.text());
      addToast('success', 'Golden Catalog', 'Catalog record deleted.');
      refreshAll();
    } catch (err) {
      addToast('error', 'Delete Failed', err.message);
    }
  };

  // ─── Bulk Upload (Excel/CSV) ─────────────────────────────
  const handleBulkFile = async (file) => {
    if (!file) return;
    setUploading(true);
    setBulkResults(null);

    try {
      const text = await file.text();
      const lines = text.trim().split('\n');
      if (lines.length < 2) {
        addToast('error', 'Invalid File', 'File must have a header row and at least one data row.');
        setUploading(false);
        return;
      }

      const headers = lines[0].split(',').map((h) => h.trim());
      const nounIdx = headers.findIndex((h) => h.toLowerCase() === 'noun');
      const modIdx = headers.findIndex((h) => h.toLowerCase() === 'modifier');
      const typeIdx = headers.findIndex((h) => h.toLowerCase() === 'request_type' || h.toLowerCase() === 'requesttype');
      const plantIdx = headers.findIndex((h) => h.toLowerCase() === 'plant');

      if (nounIdx === -1 || modIdx === -1) {
        addToast('error', 'Invalid Format', 'CSV must have Noun and Modifier columns.');
        setUploading(false);
        return;
      }

      const attrHeaders = headers.filter((_, i) => i !== nounIdx && i !== modIdx && i !== typeIdx && i !== plantIdx);
      const items = [];

      for (let i = 1; i < lines.length; i++) {
        const cols = lines[i].split(',').map((c) => c.trim().replace(/^"|"$/g, ''));
        if (cols.length < 2) continue;

        const attributes = {};
        attrHeaders.forEach((ah) => {
          const idx = headers.indexOf(ah);
          if (idx !== -1 && cols[idx]) attributes[ah] = cols[idx];
        });

        items.push({
          noun: cols[nounIdx],
          modifier: cols[modIdx],
          requestType: typeIdx !== -1 ? cols[typeIdx] || 'Single' : 'Single',
          plant: plantIdx !== -1 ? cols[plantIdx] || 'PLT1' : 'PLT1',
          attributes,
        });
      }

      const res = await fetch(`${API}/requests/bulk`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ items }),
      });

      const data = await res.json();
      setBulkResults(data);

      if (data.successful > 0) {
        addToast('success', 'Bulk Upload Complete', `${data.successful} item(s) created, ${data.duplicates} duplicate(s) flagged.`);
      } else {
        addToast('warning', 'Bulk Upload', `${data.duplicates} duplicate(s) flagged. ${data.errors} error(s).`);
      }
      refreshAll();
    } catch (e) {
      addToast('error', 'Upload Error', e.message);
    } finally {
      setUploading(false);
    }
  };

  const handleDrop = (e) => {
    e.preventDefault();
    setDragOver(false);
    const file = e.dataTransfer.files[0];
    handleBulkFile(file);
  };

  // ─── Download CSV Template ───────────────────────────────
  const downloadTemplate = () => {
    const csv = 'Noun,Modifier,Request_Type,Plant,Inside_Diameter,Outside_Diameter,Material,Type,Thread,Length\nBEARING,BALL,Single,PLT1,20MM,32MM,STEEL,,,\nBOLT,STUD,Single,PLT1,,,,HEX,M12x1.75,100MM,ALLOY STEEL\nBEARING,BALL,Plant_Extension,PLT2,20MM,32MM,STEEL,,,';
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = 'nomcat_bulk_template.csv'; a.click();
    URL.revokeObjectURL(url);
  };

  // ─── NomBot Chat Interaction ─────────────────────────────
  const askNomBot = async (msgText) => {
    const query = msgText || chatMessage;
    if (!query.trim()) return;

    setChatLogs((prev) => [...prev, { sender: 'user', text: query }]);
    setChatMessage('');
    setAskingBot(true);

    try {
      const res = await fetch(`${API}/nombot/ask`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: query }),
      });
      if (res.ok) {
        const data = await res.json();
        setChatLogs((prev) => [...prev, { sender: 'bot', text: data.reply }]);
      } else {
        setChatLogs((prev) => [...prev, { sender: 'bot', text: "Sorry, I ran into an issue connecting to the chat service." }]);
      }
    } catch (e) {
      setChatLogs((prev) => [...prev, { sender: 'bot', text: `Connection error: ${e.message}` }]);
    } finally {
      setAskingBot(false);
    }
  };

  // Filtered golden list for search dropdown
  const filteredGolden = catalog.filter((c) =>
    c.materialNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
    c.shortDescription.toLowerCase().includes(searchQuery.toLowerCase())
  );

  // Filtered Work Tray staging requests
  const filteredRequests = requests.filter((r) => {
    const matchesSearch = r.requestRefNo.toLowerCase().includes(wtSearch.toLowerCase()) ||
                          r.shortDescription.toLowerCase().includes(wtSearch.toLowerCase());
    const matchesStatus = wtStatus === '' || r.approvalStatus === wtStatus;
    const matchesPlant = wtPlant === '' || r.plant.toLowerCase() === wtPlant.toLowerCase();
    return matchesSearch && matchesStatus && matchesPlant;
  });

  // Calculate dynamic metrics for dashboard
  const approvedRate = summary.totalRequests > 0 
    ? Math.round((summary.approved / summary.totalRequests) * 100)
    : 0;
  const catalogUniquenessIndex = 100;

  // Calculate live plant distribution for bar chart
  const plantCounts = {};
  catalog.forEach(item => {
    const p = item.plant || 'PLT1';
    plantCounts[p] = (plantCounts[p] || 0) + 1;
  });
  
  const sortedPlants = Object.entries(plantCounts)
    .map(([plantCode, count]) => ({ plantCode, count }))
    .sort((a, b) => b.count - a.count)
    .slice(0, 4);

  if (sortedPlants.length === 0) {
    sortedPlants.push(
      { plantCode: 'PLT1', count: 0 },
      { plantCode: 'PLT2', count: 0 }
    );
  }
  
  const maxCount = Math.max(...sortedPlants.map(p => p.count), 1);

  // ─── Governance Insights Math ────────────────────────────
  // 1. Taxonomy Profile Distribution (in Golden Catalog)
  const taxonomyCounts = {};
  catalog.forEach(item => {
    const key = `${item.noun} / ${item.modifier}`;
    taxonomyCounts[key] = (taxonomyCounts[key] || 0) + 1;
  });
  const sortedTaxonomy = Object.entries(taxonomyCounts)
    .map(([name, count]) => ({ name, count }))
    .sort((a, b) => b.count - a.count);
  const maxTaxonomyCount = Math.max(...sortedTaxonomy.map(t => t.count), 1);

  // 2. Request Status Breakdown
  const statusCounts = {
    Validated: 0,
    InProgress: 0,
    Approved: 0,
    Rejected: 0
  };
  requests.forEach(req => {
    if (req.approvalStatus === 'Approved') {
      statusCounts.Approved++;
    } else if (req.approvalStatus === 'Rejected') {
      statusCounts.Rejected++;
    } else if (req.approvalStatus === 'Stage1_Validated') {
      statusCounts.Validated++;
    } else {
      statusCounts.InProgress++;
    }
  });

  // 3. Plant Distribution Donut Segments (circumference = 219.91)
  const plantCatalogCounts = {};
  catalog.forEach(item => {
    const p = item.plant || 'PLT1';
    plantCatalogCounts[p] = (plantCatalogCounts[p] || 0) + 1;
  });
  const totalCatalogCount = catalog.length || 1;
  const plantDonutSegments = Object.entries(plantCatalogCounts).map(([plantCode, count]) => ({
    plantCode,
    count,
    percentage: count / totalCatalogCount
  }));


  // If not logged in, show login page
  if (!currentUser) {
    return (
      <>
        <ToastContainer toasts={toasts} onDismiss={dismissToast} />
        <Login onLogin={handleLogin} />
      </>
    );
  }

  // Initials for avatar icon
  const getInitials = (name) => {
    return name.split(' ').map((n) => n[0]).join('');
  };

  const getFormattedRole = (role) => {
    return role === 'CentralCataloger' ? 'Cataloger' : role.replace(/([A-Z])/g, ' $1').trim();
  };

  // ─── RENDER ──────────────────────────────────────────────
  return (
    <>
      <ToastContainer toasts={toasts} onDismiss={dismissToast} />

      {/* ═══ LEFT SIDEBAR NAVIGATION ═══ */}
      <aside className="sidebar">
        <div className="sidebar-header">
          <div className="sidebar-logo">N</div>
          <div className="sidebar-brand-text">
            <span className="sidebar-title">NOMCAT</span>
            <span className="sidebar-subtitle">Material Cataloging</span>
          </div>
        </div>
        <div className="sidebar-menu">
          <button className={`sidebar-item ${activeTab === 'dashboard' ? 'active' : ''}`} onClick={() => setActiveTab('dashboard')}>
            <span className="sidebar-icon"><DashboardIcon /></span> Dashboard
          </button>
          <button className={`sidebar-item ${activeTab === 'catalog' ? 'active' : ''}`} onClick={() => setActiveTab('catalog')}>
            <span className="sidebar-icon"><CatalogIcon /></span> Material Catalog
          </button>
          <button className={`sidebar-item ${activeTab === 'workTray' ? 'active' : ''}`} onClick={() => setActiveTab('workTray')}>
            <span className="sidebar-icon"><WorkTrayIcon /></span> Work Tray
          </button>
          <button className={`sidebar-item ${activeTab === 'newRequest' ? 'active' : ''}`} onClick={() => setActiveTab('newRequest')}>
            <span className="sidebar-icon"><NewRequestIcon /></span> New Request
          </button>
          <button className={`sidebar-item ${activeTab === 'bulkUpload' ? 'active' : ''}`} onClick={() => setActiveTab('bulkUpload')}>
            <span className="sidebar-icon"><BulkImportIcon /></span> Bulk Import
          </button>
          <button className={`sidebar-item ${activeTab === 'reporting' ? 'active' : ''}`} onClick={() => setActiveTab('reporting')}>
            <span className="sidebar-icon"><ReportingIcon /></span> Reporting
          </button>
        </div>
      </aside>

      {/* ═══ MAIN WORKSPACE WRAPPER ═══ */}
      <div className="main-wrapper">
        
        {/* Top Header Row */}
        <header className="header">
          <div className="header-left">
            <div className="global-search">
              <span className="global-search-icon"><SearchIcon /></span>
              <input
                type="text"
                className="global-search-input"
                placeholder="Global Search..."
                value={globalSearch}
                onChange={(e) => {
                  setGlobalSearch(e.target.value);
                  if (activeTab !== 'catalog') {
                    setActiveTab('catalog');
                  }
                }}
              />
            </div>
          </div>
          <div className="header-right">
            <div className="header-action-btn-wrapper">
              <button
                className="header-action-btn"
                title="Notifications"
                onClick={() => {
                  setShowNotificationsDropdown(!showNotificationsDropdown);
                  // Mark all notifications as read when opening
                  setNotifications(prev => prev.map(n => ({ ...n, read: true })));
                }}
              >
                <BellIcon />
                {notifications.some(n => !n.read) && <span className="notification-badge" />}
              </button>

              {showNotificationsDropdown && (
                <div className="notifications-dropdown">
                  <div className="notifications-header">
                    <span>Notifications</span>
                    <button
                      className="notifications-clear-btn"
                      onClick={() => setNotifications([])}
                    >
                      Clear All
                    </button>
                  </div>
                  <div className="notifications-list">
                    {notifications.length > 0 ? (
                      notifications.map(n => (
                        <div key={n.id} className="notification-item">
                          <div className={`notification-icon-wrapper ${n.type}`}>
                            {n.type === 'success' ? <SuccessIcon /> : <WarningIcon />}
                          </div>
                          <div className="notification-content">
                            <span className="notification-title">{n.title}</span>
                            <span className="notification-message">{n.message}</span>
                            <span className="notification-time">
                              {n.timestamp.toLocaleTimeString()}
                            </span>
                          </div>
                        </div>
                      ))
                    ) : (
                      <div className="notifications-empty">
                        No notifications.
                      </div>
                    )}
                  </div>
                </div>
              )}
            </div>

            <button
              className="header-action-btn"
              title="Help/Knowledge Center"
              onClick={() => setShowHelpModal(true)}
            >
              <HelpIcon />
            </button>
            
            {/* Centered Avatar User Card */}
            <div className="header-user-profile" onClick={() => setShowProfileDropdown(!showProfileDropdown)}>
              <div className="user-avatar" style={{ backgroundColor: currentUser.avatarColor }}>
                {getInitials(currentUser.name)}
              </div>
              <div className="user-info">
                <span className="user-name">{currentUser.name}</span>
                <span className="user-role-label">{getFormattedRole(currentUser.role)}</span>
              </div>
              <span style={{ fontSize: '0.55rem', color: 'var(--text-secondary)', marginLeft: '1px' }}>▼</span>
              
              {showProfileDropdown && (
                <div className="profile-dropdown-menu">
                  <button type="button" className="dropdown-action-btn" onClick={handleLogout}>
                    🚪 Sign Out
                  </button>
                </div>
              )}
            </div>
          </div>
        </header>

        {/* main content area */}
        <main className={`main-content ${isBotOpen ? 'nombot-open' : ''}`}>

          {/* TAB: DASHBOARD (WASBOARD VIEW) */}
          {activeTab === 'dashboard' && (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem', width: '100%' }}>
              
              {/* Dashboard Sub-Tab Selector Navigation Bar */}
              <div className="insights-tab-bar" style={{ display: 'flex', gap: '1.5rem', borderBottom: '1px solid var(--border-primary)', paddingBottom: '0.5rem', marginBottom: '0.25rem' }}>
                <button
                  className={`insights-tab-btn ${dashboardSubTab === 'operations' ? 'active' : ''}`}
                  onClick={() => setDashboardSubTab('operations')}
                  style={{
                    background: 'none',
                    border: 'none',
                    color: dashboardSubTab === 'operations' ? '#6366f1' : 'var(--text-secondary)',
                    fontWeight: 600,
                    fontSize: '0.78rem',
                    cursor: 'pointer',
                    paddingBottom: '0.4rem',
                    borderBottom: dashboardSubTab === 'operations' ? '2px solid #6366f1' : '2px solid transparent',
                    transition: 'all 0.2s ease',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '0.4rem'
                  }}
                >
                  <DashboardIcon /> Operations Console
                </button>
                <button
                  className={`insights-tab-btn ${dashboardSubTab === 'insights' ? 'active' : ''}`}
                  onClick={() => setDashboardSubTab('insights')}
                  style={{
                    background: 'none',
                    border: 'none',
                    color: dashboardSubTab === 'insights' ? '#6366f1' : 'var(--text-secondary)',
                    fontWeight: 600,
                    fontSize: '0.78rem',
                    cursor: 'pointer',
                    paddingBottom: '0.4rem',
                    borderBottom: dashboardSubTab === 'insights' ? '2px solid #6366f1' : '2px solid transparent',
                    transition: 'all 0.2s ease',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '0.4rem'
                  }}
                >
                  <ReportingIcon /> Governance Analytics & Insights
                </button>
              </div>

              {dashboardSubTab === 'operations' ? (
                <div className="washboard-grid">
                  {/* Card 1: Catalog Overview */}
                  <div className="washboard-card">
                    <div className="card-header-washboard">
                      <span>Catalog Overview</span>
                      <CatalogIcon />
                    </div>
                    <div className="catalog-overview-stats">
                      <div className="overview-stat-item">
                        <span className="overview-stat-value">{summary.goldenRecords || 0}</span>
                        <span className="overview-stat-label">Golden Records</span>
                      </div>
                      <div className="overview-stat-item">
                        <span className="overview-stat-value">{profiles.length || 0}</span>
                        <span className="overview-stat-label">Taxonomy Profiles</span>
                      </div>
                    </div>
                    <span className="washboard-badge" style={{ color: 'var(--color-success)', borderColor: 'rgba(16,185,129,0.2)' }}>
                      Active Governance Enabled
                    </span>
                  </div>

                  {/* Card 2: Search Catalog */}
                  <div className="washboard-card search-box">
                    <div className="card-header-washboard">
                      <span>Search Catalog Inventory</span>
                      <SearchIcon />
                    </div>
                    <div className="washboard-search-box">
                      <input
                        type="text"
                        className="form-input search-input"
                        placeholder="Search by Material ID or short desc..."
                        value={searchQuery}
                        onFocus={() => setShowGoldenDropdown(true)}
                        onChange={(e) => {
                          setSearchQuery(e.target.value);
                          setShowGoldenDropdown(true);
                        }}
                      />
                      {showGoldenDropdown && searchQuery && (
                        <div className="dropdown-results" style={{ width: '100%' }}>
                          {filteredGolden.length === 0 ? (
                            <div style={{ padding: '0.5rem', fontSize: '0.72rem', color: 'var(--text-muted)' }}>No matches.</div>
                          ) : (
                            filteredGolden.map((rec) => (
                              <div key={rec.id} className="dropdown-item" onClick={() => {
                                setSearchQuery(rec.materialNumber);
                                setShowGoldenDropdown(false);
                                addToast('info', 'Record Detail', `${rec.materialNumber} - ${rec.shortDescription}`);
                              }} style={{ fontSize: '0.72rem' }}>
                                <strong>{rec.materialNumber}</strong> ({rec.plant}) — {rec.shortDescription}
                              </div>
                            ))
                          )}
                        </div>
                      )}
                      <button type="button" className="btn btn-primary btn-sm" onClick={() => {
                        setShowGoldenDropdown(false);
                        setSearchQuery('');
                      }}>Clear Search</button>
                    </div>
                  </div>

                  {/* Card 3: Recent Activity (Live Ingestion Stream) */}
                  <div className="washboard-card">
                    <div className="card-header-washboard">
                      <span>Live Staging Stream</span>
                      <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ verticalAlign: 'middle' }}><path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z"/></svg>
                    </div>
                    <div className="washboard-activity-list">
                      {requests.length === 0 ? (
                        <div style={{ padding: '1rem', textAlign: 'center', color: 'var(--text-muted)', fontSize: '0.72rem' }}>
                          No staging requests logged yet.
                        </div>
                      ) : (
                        requests.slice(0, 3).map((req) => (
                          <div className="activity-item" key={req.id}>
                            <span className={`activity-dot ${req.approvalStatus === 'Approved' ? '' : 'orange'}`}></span>
                            <div className="activity-text">
                              <strong>{req.requestRefNo} ({req.plant})</strong>
                              <div style={{ color: 'var(--text-secondary)', fontSize: '0.68rem', marginTop: '1px' }}>
                                {req.noun} / {req.modifier}
                              </div>
                            </div>
                            <span className="activity-time" style={{ marginLeft: 'auto' }}>
                              <StatusBadge status={req.approvalStatus} />
                            </span>
                          </div>
                        ))
                      )}
                    </div>
                  </div>

                  {/* Card 4: Dynamic Governance Metrics */}
                  <div className="washboard-card two-thirds">
                    <div className="card-header-washboard">
                      <span>Dynamic Governance Metrics</span>
                      <ReportingIcon />
                    </div>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                      <div className="washboard-progress-item">
                        <div className="progress-header">
                          <span>Staging Approval Acceptance Rate</span>
                          <span>{approvedRate}%</span>
                        </div>
                        <div className="progress-bar-bg">
                          <div className="progress-bar-fill" style={{ width: `${approvedRate}%` }}></div>
                        </div>
                      </div>
                      <div className="washboard-progress-item">
                        <div className="progress-header">
                          <span>Catalog Uniqueness Index</span>
                          <span>{catalogUniquenessIndex}%</span>
                        </div>
                        <div className="progress-bar-bg">
                          <div className="progress-bar-fill purple" style={{ width: `${catalogUniquenessIndex}%` }}></div>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Card 5: Plant Material Density Bar Chart */}
                  <div className="washboard-card">
                    <div className="card-header-washboard">
                      <span>Plant Material Density</span>
                      <ReportingIcon />
                    </div>
                    <div className="chart-container">
                      {sortedPlants.map((p) => (
                        <div className="chart-bar-group" key={p.plantCode}>
                          <div className="chart-bar-fill" style={{ height: `${(p.count / maxCount) * 80 + 10}px` }}></div>
                          <span className="chart-bar-label" style={{ fontSize: '0.62rem', fontWeight: 600 }}>{p.plantCode}</span>
                          <span style={{ fontSize: '0.58rem', color: 'var(--text-secondary)', marginTop: '-2px' }}>{p.count} items</span>
                        </div>
                      ))}
                    </div>
                    <button className="template-link" style={{ alignSelf: 'flex-end', fontSize: '0.65rem' }} onClick={() => setActiveTab('catalog')}>View Golden Catalog →</button>
                  </div>
                </div>
              ) : (
                <div className="insights-container" style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem', width: '100%' }}>
                  
                  {/* Row 1: KPI Summary cards (mini stats) */}
                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1.25rem' }}>
                    <div className="washboard-card mini-card" style={{ padding: '0.85rem 1rem', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <div>
                        <div style={{ fontSize: '0.62rem', color: 'var(--text-secondary)', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.5px' }}>Total Ingested</div>
                        <div style={{ fontSize: '1.2rem', fontWeight: 700, color: '#fff', marginTop: '0.15rem' }}>{summary.totalRequests || 0}</div>
                      </div>
                      <div style={{ background: 'rgba(99, 102, 241, 0.1)', padding: '0.4rem', borderRadius: '50%', color: '#6366f1', display: 'flex' }}><NewRequestIcon /></div>
                    </div>
                    <div className="washboard-card mini-card" style={{ padding: '0.85rem 1rem', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <div>
                        <div style={{ fontSize: '0.62rem', color: 'var(--text-secondary)', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.5px' }}>Golden Master</div>
                        <div style={{ fontSize: '1.2rem', fontWeight: 700, color: 'var(--color-success)', marginTop: '0.15rem' }}>{summary.goldenRecords || 0}</div>
                      </div>
                      <div style={{ background: 'rgba(16, 185, 129, 0.1)', padding: '0.4rem', borderRadius: '50%', color: 'var(--color-success)', display: 'flex' }}><CatalogIcon /></div>
                    </div>
                    <div className="washboard-card mini-card" style={{ padding: '0.85rem 1rem', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <div>
                        <div style={{ fontSize: '0.62rem', color: 'var(--text-secondary)', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.5px' }}>Staging Queue</div>
                        <div style={{ fontSize: '1.2rem', fontWeight: 700, color: 'var(--color-warning)', marginTop: '0.15rem' }}>{summary.pending || 0}</div>
                      </div>
                      <div style={{ background: 'rgba(245, 158, 11, 0.1)', padding: '0.4rem', borderRadius: '50%', color: 'var(--color-warning)', display: 'flex' }}><WorkTrayIcon /></div>
                    </div>
                    <div className="washboard-card mini-card" style={{ padding: '0.85rem 1rem', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <div>
                        <div style={{ fontSize: '0.62rem', color: 'var(--text-secondary)', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.5px' }}>Active Taxonomies</div>
                        <div style={{ fontSize: '1.2rem', fontWeight: 700, color: '#818cf8', marginTop: '0.15rem' }}>{profiles.length || 0}</div>
                      </div>
                      <div style={{ background: 'rgba(129, 140, 248, 0.1)', padding: '0.4rem', borderRadius: '50%', color: '#818cf8', display: 'flex' }}><SearchIcon /></div>
                    </div>
                  </div>

                  {/* Row 2: Charts Panel */}
                  <div style={{ display: 'grid', gridTemplateColumns: '1.1fr 0.9fr', gap: '1.25rem' }}>
                    
                    {/* Left Column: Taxonomy & Funnel */}
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
                      
                      {/* Chart 1: Taxonomy Profile Distribution */}
                      <div className="washboard-card">
                        <div className="card-header-washboard">
                          <span>Taxonomy Class Distribution</span>
                          <span style={{ fontSize: '0.62rem', color: 'var(--text-secondary)' }}>Golden Master Counts</span>
                        </div>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem', marginTop: '0.25rem' }}>
                          {sortedTaxonomy.map((tax, idx) => {
                            const pct = Math.round((tax.count / maxTaxonomyCount) * 100);
                            const hue = (idx * 55) % 360;
                            const barColor = `hsl(${hue}, 70%, 55%)`;
                            return (
                              <div key={tax.name} style={{ display: 'flex', flexDirection: 'column', gap: '0.25rem' }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.68rem' }}>
                                  <span style={{ fontWeight: 600, color: '#fff' }}>{tax.name}</span>
                                  <span style={{ color: 'var(--text-secondary)' }}>{tax.count} items ({pct}%)</span>
                                </div>
                                <div style={{ height: '6px', background: 'rgba(255,255,255,0.05)', borderRadius: '3px', overflow: 'hidden' }}>
                                  <div style={{ width: `${pct}%`, height: '100%', background: barColor, borderRadius: '3px', transition: 'width 0.6s cubic-bezier(0.4, 0, 0.2, 1)' }}></div>
                                </div>
                              </div>
                            );
                          })}
                        </div>
                      </div>

                      {/* Chart 2: Governance Flow Funnel */}
                      <div className="washboard-card">
                        <div className="card-header-washboard">
                          <span>Governance Lifecycle Funnel</span>
                          <span style={{ fontSize: '0.62rem', color: 'var(--text-secondary)' }}>Staging Ingestion Flow</span>
                        </div>
                        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginTop: '0.5rem', gap: '0.5rem' }}>
                          <div style={{ flex: 1, textAlign: 'center', background: 'rgba(255,255,255,0.02)', padding: '0.6rem 0.4rem', borderRadius: '4px', border: '1px solid var(--border-primary)' }}>
                            <div style={{ fontSize: '0.58rem', textTransform: 'uppercase', color: 'var(--text-secondary)', fontWeight: 600 }}>1. Ingestion</div>
                            <div style={{ fontSize: '1rem', fontWeight: 700, color: '#fff', marginTop: '0.15rem' }}>{summary.totalRequests || 0}</div>
                            <div style={{ fontSize: '0.55rem', color: 'var(--text-muted)', marginTop: '2px' }}>Total Submitted</div>
                          </div>
                          <span style={{ color: 'var(--text-muted)', fontSize: '0.75rem', fontWeight: 700 }}>→</span>
                          <div style={{ flex: 1, textAlign: 'center', background: 'rgba(245, 158, 11, 0.05)', padding: '0.6rem 0.4rem', borderRadius: '4px', border: '1px solid rgba(245, 158, 11, 0.2)' }}>
                            <div style={{ fontSize: '0.58rem', textTransform: 'uppercase', color: 'var(--color-warning)', fontWeight: 600 }}>2. Staging</div>
                            <div style={{ fontSize: '1rem', fontWeight: 700, color: 'var(--color-warning)', marginTop: '0.15rem' }}>{statusCounts.Validated + statusCounts.InProgress}</div>
                            <div style={{ fontSize: '0.55rem', color: 'var(--text-muted)', marginTop: '2px' }}>Under Review</div>
                          </div>
                          <span style={{ color: 'var(--text-muted)', fontSize: '0.75rem', fontWeight: 700 }}>→</span>
                          <div style={{ flex: 1, textAlign: 'center', background: 'rgba(16, 185, 129, 0.05)', padding: '0.6rem 0.4rem', borderRadius: '4px', border: '1px solid rgba(16, 185, 129, 0.2)' }}>
                            <div style={{ fontSize: '0.58rem', textTransform: 'uppercase', color: 'var(--color-success)', fontWeight: 600 }}>3. Golden</div>
                            <div style={{ fontSize: '1rem', fontWeight: 700, color: 'var(--color-success)', marginTop: '0.15rem' }}>{summary.goldenRecords || 0}</div>
                            <div style={{ fontSize: '0.55rem', color: 'var(--text-muted)', marginTop: '2px' }}>Promoted</div>
                          </div>
                        </div>
                        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.62rem', color: 'var(--text-secondary)', marginTop: '0.6rem', padding: '0 0.5rem' }}>
                          <span>Rejected Tickets: <strong style={{ color: 'var(--color-danger)' }}>{statusCounts.Rejected}</strong></span>
                          <span>Auto-Blocked Duplicates: <strong style={{ color: '#ec4899' }}>{summary.duplicated || 0}</strong></span>
                        </div>
                      </div>

                    </div>

                    {/* Right Column: Plant Donut & Completeness */}
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
                      
                      {/* Chart 3: Plant Distribution Ring (SVG Donut) */}
                      <div className="washboard-card">
                        <div className="card-header-washboard">
                          <span>Plant Inventory Split</span>
                          <span style={{ fontSize: '0.62rem', color: 'var(--text-secondary)' }}>Golden Master Share</span>
                        </div>
                        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-around', marginTop: '0.5rem' }}>
                          <div style={{ position: 'relative', width: '90px', height: '90px' }}>
                            <svg viewBox="0 0 100 100" style={{ transform: 'rotate(-90deg)', width: '100%', height: '100%' }}>
                              <circle cx="50" cy="50" r="35" fill="transparent" stroke="rgba(255,255,255,0.05)" strokeWidth="12" />
                              
                              {(() => {
                                let accumPct = 0;
                                const strokeColors = ['#6366f1', '#10b981', '#f59e0b', '#ec4899'];
                                return plantDonutSegments.map((seg, idx) => {
                                  const c = 2 * Math.PI * 35; // 219.91
                                  const dashArray = `${c * seg.percentage} ${c}`;
                                  const dashOffset = -(c * accumPct);
                                  accumPct += seg.percentage;
                                  return (
                                    <circle
                                      key={seg.plantCode}
                                      cx="50"
                                      cy="50"
                                      r="35"
                                      fill="transparent"
                                      stroke={strokeColors[idx % strokeColors.length]}
                                      strokeWidth="12"
                                      strokeDasharray={dashArray}
                                      strokeDashoffset={dashOffset}
                                      style={{ transition: 'all 0.6s ease' }}
                                    />
                                  );
                                });
                              })()}
                            </svg>
                            <div style={{ position: 'absolute', top: 0, left: 0, right: 0, bottom: 0, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' }}>
                              <span style={{ fontSize: '0.85rem', fontWeight: 700, color: '#fff' }}>{summary.goldenRecords || 0}</span>
                              <span style={{ fontSize: '0.45rem', color: 'var(--text-secondary)', textTransform: 'uppercase' }}>Items</span>
                            </div>
                          </div>

                          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
                            {plantDonutSegments.map((seg, idx) => {
                              const strokeColors = ['#6366f1', '#10b981', '#f59e0b', '#ec4899'];
                              const pct = Math.round(seg.percentage * 100);
                              return (
                                <div key={seg.plantCode} style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', fontSize: '0.68rem' }}>
                                  <span style={{ width: '8px', height: '8px', borderRadius: '50%', background: strokeColors[idx % strokeColors.length] }}></span>
                                  <span style={{ fontWeight: 600, color: '#fff' }}>{seg.plantCode}:</span>
                                  <span style={{ color: 'var(--text-secondary)' }}>{seg.count} ({pct}%)</span>
                                </div>
                              );
                            })}
                          </div>
                        </div>
                      </div>

                      {/* Chart 4: Governance Quality & Completeness Index */}
                      <div className="washboard-card">
                        <div className="card-header-washboard">
                          <span>Metadata Quality Index</span>
                          <span style={{ fontSize: '0.62rem', color: 'var(--text-secondary)' }}>Completeness Ratio</span>
                        </div>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginTop: '0.5rem' }}>
                          <div style={{ position: 'relative', width: '65px', height: '65px' }}>
                            <svg viewBox="0 0 100 100" style={{ transform: 'rotate(-90deg)', width: '100%', height: '100%' }}>
                              <circle cx="50" cy="50" r="40" fill="transparent" stroke="rgba(255,255,255,0.05)" strokeWidth="8" />
                              <circle cx="50" cy="50" r="40" fill="transparent" stroke="#10b981" strokeWidth="8" strokeDasharray="251.2" strokeDashoffset="0" />
                            </svg>
                            <div style={{ position: 'absolute', top: 0, left: 0, right: 0, bottom: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '0.78rem', fontWeight: 700, color: 'var(--color-success)' }}>
                              100%
                            </div>
                          </div>
                          <div style={{ flex: 1 }}>
                            <div style={{ fontSize: '0.72rem', fontWeight: 600, color: '#fff' }}>Mandatory Fields Integrity</div>
                            <p style={{ fontSize: '0.6rem', color: 'var(--text-secondary)', marginTop: '2px', lineHeight: 1.3 }}>
                              All active Golden Master Records satisfy 100% of defined attribute rules and nomenclature standards.
                            </p>
                          </div>
                        </div>
                      </div>

                    </div>

                  </div>

                </div>
              )}
            </div>
          )}

          {/* TAB: MATERIAL CATALOG (GOLDEN CATALOG) */}
          {activeTab === 'catalog' && (() => {
            const filteredCatalog = catalog.filter(rec => {
              if (!globalSearch.trim()) return true;
              const query = globalSearch.toLowerCase();
              return rec.materialNumber.toLowerCase().includes(query) ||
                     rec.noun.toLowerCase().includes(query) ||
                     rec.modifier.toLowerCase().includes(query) ||
                     rec.plant.toLowerCase().includes(query) ||
                     rec.shortDescription.toLowerCase().includes(query) ||
                     (rec.longDescription && rec.longDescription.toLowerCase().includes(query)) ||
                     rec.sourceRequestRef.toLowerCase().includes(query);
            });

            return (
              <div className="worktray-table-container">
                <div className="worktray-table-title">
                  <span>🏆 Golden Master Catalog Records</span>
                  <span className="lifecycle-label">
                    {globalSearch.trim() ? `${filteredCatalog.length} Matching record(s)` : `${catalog.length} Active record(s)`}
                  </span>
                </div>
                <div className="data-table-wrapper">
                  <table className="data-table">
                    <thead>
                      <tr>
                        <th>Material Number</th>
                        <th>Noun</th>
                        <th>Modifier</th>
                        <th>Plant</th>
                        <th>Standardized Description</th>
                        <th>Source Request</th>
                        <th>Cataloged Date</th>
                        <th style={{ textAlign: 'center' }}>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {filteredCatalog.length > 0 ? (
                        filteredCatalog.map((rec) => (
                          <tr key={rec.id}>
                            <td style={{ fontWeight: 600, color: 'var(--color-success)' }}>{rec.materialNumber}</td>
                            <td>{rec.noun}</td>
                            <td>{rec.modifier}</td>
                            <td style={{ fontWeight: 600, color: '#6366f1' }}>{rec.plant}</td>
                            <td style={{ fontFamily: 'monospace' }}>{rec.shortDescription}</td>
                            <td>{rec.sourceRequestRef}</td>
                            <td>{new Date(rec.approvedAt).toLocaleDateString()}</td>
                            <td style={{ textAlign: 'center' }}>
                              <button
                                className="btn btn-danger btn-sm"
                                style={{ padding: '0.2rem 0.35rem', background: 'transparent', border: '1px solid rgba(239, 68, 68, 0.2)', color: 'var(--color-danger)' }}
                                onClick={() => deleteGoldenRecord(rec.id)}
                                title="Delete Golden Master Record"
                              >
                                <svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                              </button>
                            </td>
                          </tr>
                        ))
                      ) : (
                        <tr>
                          <td colSpan="8" style={{ textAlign: 'center', color: 'var(--text-secondary)', padding: '2rem' }}>
                            No matching records found.
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
              </div>
            );
          })()}

          {/* TAB: WORK TRAY (MATERIAL STAGING QUEUE) */}
          {activeTab === 'workTray' && (
            <div className="createrequest-container">
              <div className="worktray-header-row">
                <div className="worktray-title-block">
                  <h2>Work Tray - Material Staging</h2>
                </div>
                <div className="worktray-controls">
                  <input
                    type="text"
                    className="form-input"
                    placeholder="Search staging queue..."
                    value={wtSearch}
                    onChange={(e) => setWtSearch(e.target.value)}
                    style={{ maxWidth: '180px' }}
                  />
                  <select
                    className="form-select"
                    value={wtStatus}
                    onChange={(e) => setWtStatus(e.target.value)}
                    style={{ maxWidth: '140px' }}
                  >
                    <option value="">All Statuses</option>
                    <option value="Stage1_Validated">Stage1 Validated</option>
                    <option value="In_Progress">In Progress</option>
                    <option value="Approved">Approved</option>
                    <option value="Rejected">Rejected</option>
                  </select>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="Filter by Plant..."
                    value={wtPlant}
                    onChange={(e) => setWtPlant(e.target.value)}
                    style={{ maxWidth: '120px' }}
                  />
                  <button className="btn btn-outline" style={{ padding: '0.4rem 0.85rem' }} onClick={() => setActiveTab('newRequest')}>Submit to NMSR</button>
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: selectedRequest ? '1.1fr 0.9fr' : '1fr', gap: '1.25rem', alignItems: 'start' }}>
                
                {/* Left Side: Staging Table */}
                <div className="worktray-table-container">
                  <div className="worktray-table-title">
                    <span>Material Staging Queue</span>
                    <button className="btn btn-outline btn-sm" onClick={refreshAll} style={{ padding: '0.2rem 0.5rem' }}>↻ Refresh</button>
                  </div>

                  <div className="data-table-wrapper">
                    <table className="data-table">
                      <thead>
                        <tr>
                          <th style={{ width: '40px' }}><span className="custom-checkbox"></span></th>
                          <th>REF ↑</th>
                          <th>NOUN</th>
                          <th>MODIFIER</th>
                          <th>PLANT</th>
                          <th>PIPELINE</th>
                          <th>STATUS</th>
                          <th>ACTIONS</th>
                        </tr>
                      </thead>
                      <tbody>
                        {filteredRequests.map((req) => (
                          <tr
                            key={req.id}
                            onClick={() => setSelectedRequest(req)}
                            style={{ cursor: 'pointer', background: selectedRequest?.id === req.id ? 'rgba(79, 70, 229, 0.08)' : '' }}
                          >
                            <td><span className="custom-checkbox"></span></td>
                            <td style={{ fontWeight: 600 }}>{req.requestRefNo}</td>
                            <td>{req.noun}</td>
                            <td>{req.modifier}</td>
                            <td style={{ color: '#818cf8', fontWeight: 600 }}>{req.plant}</td>
                            <td>
                              {req.requestType === 'Plant_Extension' ? 'Plant Extension (3-Stage)' : 'Standard Pipeline (4-Stage)'}
                            </td>
                            <td><StatusBadge status={req.approvalStatus} /></td>
                            <td>
                              <div className="btn-group" style={{ display: 'flex', gap: '0.35rem', alignItems: 'center' }} onClick={(e) => e.stopPropagation()}>
                                {req.approvalStatus !== 'Approved' && req.approvalStatus !== 'Rejected' && req.approvalStatus !== 'Duplicated' ? (
                                  <>
                                    <button className="btn-approve-outline" onClick={() => handleApproval(req.id, 'Approve')}>Approve</button>
                                    <button className="btn-reject-outline" onClick={() => handleApproval(req.id, 'Reject')}>Reject</button>
                                  </>
                                ) : (
                                  <span style={{ color: 'var(--text-muted)', fontSize: '0.65rem' }}>Completed</span>
                                )}
                                <button
                                  className="btn btn-danger btn-sm"
                                  style={{ padding: '0.2rem 0.35rem', background: 'transparent', border: '1px solid rgba(239, 68, 68, 0.2)', color: 'var(--color-danger)' }}
                                  onClick={() => deleteStagingRequest(req.id)}
                                  title="Delete Staging Request"
                                >
                                  <svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                </button>
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  {/* Table Pagination row */}
                  <div className="table-pagination-row">
                    <span>1-{filteredRequests.length} of {filteredRequests.length} items</span>
                    <div className="pagination-controls">
                      <button className="pagination-btn">&lt;</button>
                      <button className="pagination-btn active">1</button>
                      <button className="pagination-btn">&gt;</button>
                    </div>
                  </div>
                </div>

                {/* Right Side: Selected Request Details & Timeline Drawer */}
                {selectedRequest && (
                  <div className="request-details-drawer">
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', borderBottom: '1px solid var(--border-primary)', paddingBottom: '0.5rem' }}>
                      <h3 style={{ fontSize: '0.8rem', fontWeight: 700, color: '#fff' }}>📋 Request Details</h3>
                      <button onClick={() => setSelectedRequest(null)} style={{ background: 'none', border: 'none', color: 'var(--text-secondary)', cursor: 'pointer', fontSize: '1.25rem', padding: '0 0.25rem' }}>×</button>
                    </div>
                    
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem', fontSize: '0.74rem' }}>
                      <div><strong>Reference Number:</strong> <span style={{ color: '#818cf8', fontWeight: 600 }}>{selectedRequest.requestRefNo}</span></div>
                      <div><strong>Plant Assignment:</strong> <span style={{ color: '#10b981', fontWeight: 600 }}>{selectedRequest.plant}</span></div>
                      <div><strong>Request Type:</strong> {selectedRequest.requestType.replace(/_/g, ' ')}</div>
                      <div><strong>Nomenclature description:</strong></div>
                      <div className="preview-desc-text" style={{ fontSize: '0.72rem', marginTop: '2px' }}>{selectedRequest.shortDescription}</div>
                    </div>

                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.25rem' }}>
                      <div className="form-section-title-centered" style={{ margin: '0.25rem 0', fontSize: '0.65rem' }}>Attributes values</div>
                      <div style={{ maxHeight: '110px', overflowY: 'auto', background: 'var(--bg-input)', padding: '0.5rem', borderRadius: '4px', border: '1px solid var(--border-primary)' }}>
                        {Object.entries(JSON.parse(selectedRequest.jsonAttributeValues || "{}")).map(([k, v]) => (
                          <div key={k} style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.7rem', borderBottom: '1px solid var(--border-subtle)', padding: '0.2rem 0' }}>
                            <span style={{ color: 'var(--text-secondary)' }}>{k.replace(/_/g, ' ')}:</span>
                            <strong style={{ color: '#fff' }}>{v}</strong>
                          </div>
                        ))}
                      </div>
                    </div>

                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.25rem' }}>
                      <div className="form-section-title-centered" style={{ margin: '0.25rem 0', fontSize: '0.65rem' }}>Approval Audit Trail</div>
                      <ApprovalTimeline request={selectedRequest} />
                    </div>
                  </div>
                )}

              </div>
            </div>
          )}

          {/* TAB: NEW REQUEST */}
          {activeTab === 'newRequest' && (
            <div className="createrequest-container">
              <div className="form-two-columns">
                
                {/* Left Side: Create Form */}
                <div className="form-section-card">
                  
                  {/* AI Assist Panel */}
                  <div className="ai-assist-panel" style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                    <div className="ai-assist-header">
                      <span>🤖 AI Smart Assist</span>
                      {aiResult && (
                        <span className={`confidence-badge ${aiResult.confidence >= 0.9 ? 'high' : aiResult.confidence >= 0.7 ? 'medium' : 'low'}`}>
                          {Math.round(aiResult.confidence * 100)}% confidence
                        </span>
                      )}
                    </div>
                    
                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                      <div>
                        <div style={{ fontSize: '0.68rem', color: 'var(--text-secondary)', marginBottom: '0.25rem', fontWeight: 600 }}>Option A: Describe Material</div>
                        <textarea
                          className="ai-assist-textarea"
                          placeholder="Describe in plain language... e.g. '12mm ID steel ball bearing for plant 2'"
                          value={aiDescription}
                          onChange={(e) => setAiDescription(e.target.value)}
                          style={{ height: '70px', resize: 'none' }}
                        />
                        <button
                          type="button"
                          className="btn-ai"
                          onClick={handleAiClassify}
                          disabled={aiClassifying || !aiDescription.trim()}
                          style={{ width: '100%', marginTop: '0.4rem', fontSize: '0.68rem' }}
                        >
                          {aiClassifying ? 'Classifying...' : '🧠 Classify Text'}
                        </button>
                      </div>

                      <div style={{ display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
                        <div style={{ fontSize: '0.68rem', color: 'var(--text-secondary)', marginBottom: '0.25rem', fontWeight: 600 }}>Option B: Upload Photo / Image</div>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem', flex: 1, justifyContent: 'center', alignItems: 'center', border: '1px dashed var(--border-primary)', borderRadius: '6px', padding: '0.4rem', background: 'rgba(255,255,255,0.01)', minHeight: '70px', cursor: 'pointer', position: 'relative' }} onClick={() => document.getElementById('image-upload-input').click()}>
                          {imagePreview ? (
                            <img src={imagePreview} alt="Preview" style={{ maxHeight: '60px', borderRadius: '4px', maxWidth: '100%', objectFit: 'contain' }} />
                          ) : (
                            <>
                              <span style={{ fontSize: '1.2rem' }}>📷</span>
                              <span style={{ fontSize: '0.55rem', color: 'var(--text-muted)', textAlign: 'center' }}>Choose Photo / Upload</span>
                            </>
                          )}
                          <input
                            id="image-upload-input"
                            type="file"
                            accept="image/*"
                            onChange={(e) => {
                              const file = e.target.files[0];
                              if (file) {
                                setImageFile(file);
                                setImagePreview(URL.createObjectURL(file));
                              }
                            }}
                            onClick={(e) => e.stopPropagation()}
                            style={{ display: 'none' }}
                          />
                        </div>
                        <button
                          type="button"
                          className="btn-ai"
                          onClick={handleImageClassify}
                          disabled={imageClassifying || !imageFile}
                          style={{ width: '100%', marginTop: '0.4rem', fontSize: '0.68rem' }}
                        >
                          {imageClassifying ? 'Analyzing Image...' : '📷 Classify Image'}
                        </button>
                      </div>
                    </div>
                  </div>

                   {/* Similarity Warning */}
                  {similarityWarning && similarityWarning.length > 0 && (
                    <div className="similarity-warning">
                      <div className="similarity-warning-header">
                        ⚠️ Similar materials found in Golden Catalog
                      </div>
                      {similarityWarning.slice(0, 3).map((item, idx) => (
                        <div className="similarity-item" key={idx}>
                          <span><strong>{item.materialNumber}</strong> ({item.plant}) — {item.shortDescription}</span>
                          <span className="similarity-score">{Math.round(item.similarity * 100)}%</span>
                        </div>
                      ))}
                      <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.25rem' }}>
                        <button type="button" className="btn btn-outline btn-sm" onClick={() => {
                          const highestMatch = similarityWarning[0];
                          const matchRec = catalog.find(c => c.materialNumber === highestMatch.materialNumber);
                          if (matchRec) {
                            handleSelectGoldenRecord(matchRec);
                            setRequestType('Plant_Extension');
                            addToast('info', 'Requisition Recommendation', `Switched to Plant Extension for ${matchRec.materialNumber}`);
                          }
                          setSimilarityWarning(null);
                        }}>
                          Use Plant Extension
                        </button>
                        <button type="button" className="btn btn-outline btn-sm" onClick={() => setSimilarityWarning(null)}>
                          Proceed Anyway
                        </button>
                      </div>
                    </div>
                  )}

                  <div className="form-section-title-centered">Noun/Modifier Definition</div>
                  <div className="form-row-aligned">
                    <label className="form-label-aligned">Noun <span className="mandatory">*</span></label>
                    <select className="form-select-aligned" value={selectedProfile} onChange={(e) => handleProfileChange(e.target.value)} required>
                      <option value="">— Select Classification —</option>
                      {profiles.map((p) => (
                        <option key={`${p.noun}|${p.modifier}`} value={`${p.noun}|${p.modifier}`}>{p.noun}</option>
                      ))}
                    </select>
                  </div>
                  <div className="form-row-aligned">
                    <label className="form-label-aligned">Modifier <span className="mandatory">*</span></label>
                    <select className="form-select-aligned" value={selectedProfile} onChange={(e) => handleProfileChange(e.target.value)} required>
                      <option value="">— Select Modifier —</option>
                      {profiles.map((p) => (
                        <option key={`${p.noun}|${p.modifier}`} value={`${p.noun}|${p.modifier}`}>{p.modifier}</option>
                      ))}
                    </select>
                  </div>

                  <div className="form-section-title-centered">Request Details</div>
                  <div className="form-row-aligned">
                    <label className="form-label-aligned">Request Type <span className="mandatory">*</span></label>
                    <select className="form-select-aligned" value={requestType} onChange={(e) => {
                      setRequestType(e.target.value);
                      setSelectedProfile('');
                      setSchema(null);
                      setAttrValues({});
                      setSelectedGoldenRecord(null);
                      setSearchQuery('');
                    }}>
                      <option value="Single">Single Catalog Item</option>
                      <option value="Multiple">Multiple Items Ingestion</option>
                      <option value="Modification">Modification</option>
                      <option value="Plant_Extension">Plant Specific Extension</option>
                    </select>
                  </div>
                  <div className="form-row-aligned">
                    <label className="form-label-aligned">Priority</label>
                    <select className="form-select-aligned" value={priority} onChange={(e) => setPriority(e.target.value)}>
                      <option value="Standard">Standard</option>
                      <option value="Urgent">Urgent</option>
                    </select>
                  </div>
                  <div className="form-row-aligned">
                    <label className="form-label-aligned">Expected Date</label>
                    <input type="date" className="form-input-aligned" value={expectedDate} onChange={(e) => setExpectedDate(e.target.value)} />
                  </div>
                  <div className="form-row-aligned">
                    <label className="form-label-aligned">Target Plant <span className="mandatory">*</span></label>
                    <input type="text" className="form-input-aligned" value={plant} onChange={(e) => setPlant(e.target.value.toUpperCase())} required />
                  </div>

                  {requestType === 'Plant_Extension' && (
                    <div className="form-row-aligned search-box" style={{ gridTemplateColumns: '140px 180px' }}>
                      <label className="form-label-aligned">Search Active <span className="mandatory">*</span></label>
                      <div style={{ position: 'relative', width: '100%' }}>
                        <input
                          className="form-input-aligned"
                          type="text"
                          placeholder="Search MAT-..."
                          value={searchQuery}
                          onFocus={() => setShowGoldenDropdown(true)}
                          onChange={(e) => {
                            setSearchQuery(e.target.value);
                            setShowGoldenDropdown(true);
                          }}
                        />
                        {showGoldenDropdown && searchQuery && (
                          <div className="dropdown-results" style={{ width: '180px' }}>
                            {filteredGolden.length === 0 ? (
                              <div style={{ padding: '0.4rem', fontSize: '0.72rem', color: 'var(--text-muted)' }}>No match.</div>
                            ) : (
                              filteredGolden.map((rec) => (
                                <div key={rec.id} className="dropdown-item" onClick={() => handleSelectGoldenRecord(rec)} style={{ fontSize: '0.72rem' }}>
                                  {rec.materialNumber} ({rec.plant})
                                </div>
                              ))
                            )}
                          </div>
                        )}
                      </div>
                    </div>
                  )}

                  {schema && (
                    <>
                      <div className="form-section-title-centered">Part Attributes</div>
                      {schema.fields.map((field) => (
                        <div className="form-row-aligned" key={field.attributeName}>
                          <label className="form-label-aligned">
                            {field.attributeName.replace(/_/g, ' ')}
                            {field.isMandatory && <span className="mandatory">*</span>}
                          </label>
                          <input
                            className="form-input-aligned"
                            type="text"
                            placeholder={requestType === 'Plant_Extension' ? 'N/A' : `Enter ${field.attributeName.replace(/_/g, ' ').toLowerCase()}`}
                            value={attrValues[field.attributeName] || ''}
                            readOnly={requestType === 'Plant_Extension'}
                            disabled={requestType === 'Plant_Extension'}
                            onChange={(e) => setAttrValues((prev) => ({ ...prev, [field.attributeName]: e.target.value }))}
                            required={field.isMandatory && requestType !== 'Plant_Extension'}
                          />
                        </div>
                      ))}
                    </>
                  )}
                  
                  <div className="form-actions-aligned">
                    <button type="button" className="btn btn-outline btn-sm" onClick={() => {
                      setSelectedProfile(''); setSchema(null); setAttrValues({}); setSelectedGoldenRecord(null); setSearchQuery('');
                    }}>Cancel</button>
                    <button type="submit" className="btn btn-primary btn-sm" onClick={handleSubmit} disabled={!schema || submitting}>
                      Submit Request
                    </button>
                  </div>
                </div>

                {/* Right Side: Description Preview Card */}
                <div className="form-section-card">
                  <div className="form-section-title-centered" style={{ border: 'none' }}>Generated Description Preview</div>
                  <div className="preview-image-box">📦</div>
                  
                  <div className="preview-row-aligned">
                    <span className="preview-row-label-aligned">Generated Noun:</span>
                    <span className="preview-row-value-aligned">{selectedProfile ? selectedProfile.split('|')[0] : 'BEARING'}</span>
                  </div>
                  <div className="preview-row-aligned">
                    <span className="preview-row-label-aligned">Generated Modifier:</span>
                    <span className="preview-row-value-aligned">{selectedProfile ? selectedProfile.split('|')[1] : 'BALL'}</span>
                  </div>
                  
                  <div className="preview-formatted-title">Formatted Description:</div>
                  <div className="preview-formatted-block">
                    {schema ? (
                      <>
                        {selectedProfile.split('|')[0]}, {selectedProfile.split('|')[1]}
                        {Object.entries(attrValues).filter(([, v]) => v.trim()).map(([k, v]) => (
                          <span key={k}>: {v.toUpperCase()}</span>
                        ))}
                      </>
                    ) : 'BEARING, BALL: 12: 15: STEEL'}
                  </div>
                  
                  <button type="button" className="btn-preview-refresh">↻ Refresh Preview</button>
                </div>

                {/* AI Audit Report Card */}
                {selectedProfile && schema && (
                  <div className="form-section-card" style={{ marginTop: '1.25rem' }}>
                    <div className="ai-assist-header" style={{ marginBottom: '0.5rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span style={{ fontWeight: 700, color: '#fff', fontSize: '0.74rem' }}>🛡️ AI Data Steward Audit</span>
                      <button
                        type="button"
                        className="btn-ai"
                        onClick={handleAiAudit}
                        disabled={auditing}
                        style={{ fontSize: '0.65rem', padding: '0.2rem 0.6rem', marginTop: 0 }}
                      >
                        {auditing ? 'Auditing...' : 'Run AI Audit'}
                      </button>
                    </div>

                    {auditReport ? (
                      <div
                        className="audit-report-box"
                        style={{
                          fontSize: '0.7rem',
                          lineHeight: '1.35',
                          background: 'rgba(99, 102, 241, 0.05)',
                          border: '1px solid rgba(99, 102, 241, 0.2)',
                          borderRadius: '4px',
                          padding: '0.6rem',
                          maxHeight: '150px',
                          overflowY: 'auto',
                          color: '#e2e8f0',
                          whiteSpace: 'pre-line',
                          textAlign: 'left'
                        }}
                      >
                        {renderMarkdown(auditReport)}
                      </div>
                    ) : (
                      <div style={{ fontSize: '0.68rem', color: 'var(--text-muted)', textAlign: 'center', padding: '1rem 0' }}>
                        No audit report run yet. Click 'Run AI Audit' to evaluate metadata quality.
                      </div>
                    )}
                  </div>
                )}

              </div>
            </div>
          )}

          {/* TAB: BULK UPLOAD */}
          {activeTab === 'bulkUpload' && (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem', width: '100%' }}>
              
              <div style={{ display: 'grid', gridTemplateColumns: '1.2fr 1.8fr', gap: '1.25rem' }}>
                
                {/* Left Card: File Upload */}
                <div className="panel" style={{ height: 'fit-content' }}>
                  <div className="panel-header">
                    <div className="panel-title">📤 CSV Import</div>
                    <button className="template-link" style={{ fontSize: '0.65rem' }} onClick={downloadTemplate}>⬇ Template</button>
                  </div>
                  <div className="panel-body" style={{ padding: '1rem' }}>
                    <div
                      className={`upload-zone ${dragOver ? 'drag-over' : ''}`}
                      onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
                      onDragLeave={() => setDragOver(false)}
                      onDrop={handleDrop}
                      onClick={() => fileInputRef.current?.click()}
                      style={{ padding: '2rem 1rem' }}
                    >
                      <div style={{ fontSize: '2rem', marginBottom: '0.5rem' }}>📁</div>
                      <div style={{ fontWeight: 600, color: '#fff', fontSize: '0.74rem', textAlign: 'center' }}>
                        {uploading ? 'Processing CSV...' : 'Drop CSV or Browse'}
                      </div>
                      <input
                        ref={fileInputRef}
                        type="file"
                        accept=".csv"
                        style={{ display: 'none' }}
                        onChange={(e) => handleBulkFile(e.target.files[0])}
                      />
                    </div>
                  </div>
                </div>

                {/* Right Card: AI Cleanser */}
                <div className="panel">
                  <div className="panel-header">
                    <div className="panel-title">✨ AI Legacy Description Cleanser</div>
                    <span style={{ fontSize: '0.62rem', color: 'var(--text-secondary)' }}>Standardize free text items</span>
                  </div>
                  <div className="panel-body" style={{ padding: '1rem', display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
                    <textarea
                      placeholder="Paste legacy material descriptions here (one per line)...&#10;e.g.&#10;BRG BAL 25*52 ST FOR PLT3&#10;GATE VALVE 2IN FLG WCB"
                      value={rawDescriptions}
                      onChange={(e) => setRawDescriptions(e.target.value)}
                      style={{
                        width: '100%',
                        height: '100px',
                        background: 'var(--bg-input)',
                        color: '#fff',
                        border: '1px solid var(--border-primary)',
                        borderRadius: '4px',
                        padding: '0.6rem',
                        fontSize: '0.74rem',
                        fontFamily: 'monospace',
                        resize: 'vertical'
                      }}
                    />
                    <button
                      type="button"
                      className="btn-ai"
                      onClick={handleBulkClean}
                      disabled={cleansing || !rawDescriptions.trim()}
                      style={{ alignSelf: 'flex-start', marginTop: 0 }}
                    >
                      {cleansing ? 'Cleansing & Standardizing...' : '✨ Clean & Standardize descriptions'}
                    </button>

                    {cleansedPreview && cleansedPreview.length > 0 && (
                      <div style={{ display: 'flex', flexDirection: 'column', gap: '0.6rem', marginTop: '0.25rem' }}>
                        <div style={{ fontSize: '0.74rem', fontWeight: 600, color: '#fff' }}>Preview Cleansed Materials ({cleansedPreview.length})</div>
                        <div style={{ maxHeight: '180px', overflowY: 'auto', border: '1px solid var(--border-primary)', borderRadius: '4px' }}>
                          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.68rem', textAlign: 'left' }}>
                            <thead>
                              <tr style={{ background: 'rgba(255,255,255,0.03)', borderBottom: '1px solid var(--border-primary)' }}>
                                <th style={{ padding: '0.4rem' }}>Noun/Mod</th>
                                <th style={{ padding: '0.4rem' }}>Plant</th>
                                <th style={{ padding: '0.4rem' }}>Standardized Description</th>
                              </tr>
                            </thead>
                            <tbody>
                              {cleansedPreview.map((item, idx) => (
                                <tr key={idx} style={{ borderBottom: '1px solid var(--border-subtle)' }}>
                                  <td style={{ padding: '0.4rem', fontWeight: 600, color: '#fff' }}>{item.noun} / {item.modifier}</td>
                                  <td style={{ padding: '0.4rem', color: 'var(--text-secondary)' }}>{item.plant}</td>
                                  <td style={{ padding: '0.4rem', color: 'var(--color-success)' }}>{item.generatedDescription}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                        <div style={{ display: 'flex', gap: '0.5rem', alignSelf: 'flex-end' }}>
                          <button type="button" className="btn btn-outline btn-sm" onClick={() => setCleansedPreview(null)}>Discard</button>
                          <button type="button" className="btn btn-primary btn-sm" onClick={handleSubmitCleansed} disabled={uploading}>
                            {uploading ? 'Importing...' : 'Confirm & Import to Staging'}
                          </button>
                        </div>
                      </div>
                    )}
                  </div>
                </div>

              </div>

              {/* Bulk import results (Errors/Duplicates summary) */}
              {bulkResults && (
                <div className="panel" style={{ padding: '1rem', marginTop: '0.5rem' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <div style={{ fontSize: '0.78rem', fontWeight: 700, color: '#fff' }}>Bulk Import Result Report</div>
                    <button className="btn btn-outline btn-xs" onClick={() => setBulkResults(null)}>Dismiss</button>
                  </div>
                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem', marginTop: '0.5rem', fontSize: '0.72rem' }}>
                    <div style={{ background: 'rgba(16, 185, 129, 0.05)', padding: '0.5rem', borderRadius: '4px', border: '1px solid rgba(16, 185, 129, 0.2)' }}>
                      <span style={{ color: 'var(--color-success)', fontWeight: 700 }}>{bulkResults.successful}</span> Ingested to Staging
                    </div>
                    <div style={{ background: 'rgba(239, 68, 68, 0.05)', padding: '0.5rem', borderRadius: '4px', border: '1px solid rgba(239, 68, 68, 0.2)' }}>
                      <span style={{ color: 'var(--color-danger)', fontWeight: 700 }}>{bulkResults.duplicates}</span> Duplicates Flagged
                    </div>
                    <div style={{ background: 'rgba(245, 158, 11, 0.05)', padding: '0.5rem', borderRadius: '4px', border: '1px solid rgba(245, 158, 11, 0.2)' }}>
                      <span style={{ color: 'var(--color-warning)', fontWeight: 700 }}>{bulkResults.errors}</span> Validation Failures
                    </div>
                    <div style={{ background: 'rgba(255,255,255,0.02)', padding: '0.5rem', borderRadius: '4px', border: '1px solid var(--border-primary)' }}>
                      Total Parsed: <span style={{ fontWeight: 700 }}>{bulkResults.totalProcessed}</span>
                    </div>
                  </div>
                  {bulkResults.errorMessages && bulkResults.errorMessages.length > 0 && (
                    <div style={{ marginTop: '0.6rem', maxHeight: '100px', overflowY: 'auto', background: 'rgba(0,0,0,0.1)', padding: '0.4rem', borderRadius: '4px', fontSize: '0.62rem', fontFamily: 'monospace', color: 'var(--text-secondary)' }}>
                      {bulkResults.errorMessages.map((m, idx) => <div key={idx}>• {m}</div>)}
                    </div>
                  )}
                </div>
              )}

            </div>
          )}

          {/* TAB: REPORTING */}
          {activeTab === 'reporting' && (
            <div className="panel">
              <div className="panel-header">
                <div className="panel-title">📊 Reporting Metrics</div>
                <button className="btn btn-primary btn-sm" onClick={() => window.open(`${API}/reporting/staging-export`, '_blank')}>
                  📥 Export Staging Grid (CSV)
                </button>
              </div>
              <div className="panel-body">
                <div className="catalog-overview-stats" style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem' }}>
                  <div className="stat-card">
                    <span className="stat-value indigo">{summary.totalRequests || 0}</span>
                    <span className="stat-label">Registrations</span>
                  </div>
                  <div className="stat-card">
                    <span className="stat-value emerald">{summary.approved || 0}</span>
                    <span className="stat-label">Approved Records</span>
                  </div>
                  <div className="stat-card">
                    <span className="stat-value red">{summary.duplicated || 0}</span>
                    <span className="stat-label">Duplications Blocked</span>
                  </div>
                  <div className="stat-card">
                    <span className="stat-value cyan">{summary.exports || 0}</span>
                    <span className="stat-label">Bridge Exports Logged</span>
                  </div>
                </div>
              </div>
            </div>
          )}

        </main>

        {/* 🤖 NOMBOT CHAT LOG PANEL (SLIDE OUT) 🤖 */}
        {isBotOpen && (
          <aside className="nombot-panel">
            <div className="nombot-header">
              <div className="nombot-title" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>Chat Log <span className="ai-powered-badge">✨ Gemini AI</span></div>
              <button style={{ background: 'none', border: 'none', color: 'var(--text-secondary)', cursor: 'pointer', fontSize: '1.25rem' }} onClick={() => setIsBotOpen(false)}>×</button>
            </div>
            
            <div className="nombot-body">
              {/* Chat Log User Info details */}
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', background: 'rgba(255,255,255,0.02)', padding: '0.5rem', borderRadius: '4px', border: '1px solid var(--border-primary)', marginBottom: '0.5rem' }}>
                <div style={{ width: '28px', height: '28px', borderRadius: '50%', background: '#3b82f6', color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 'bold', fontSize: '0.75rem' }}>NB</div>
                <div style={{ display: 'flex', flexDirection: 'column', lineStyle: '1.1' }}>
                  <span style={{ fontSize: '0.75rem', fontWeight: 600, color: '#fff' }}>NomBot</span>
                  <span style={{ fontSize: '0.55rem', color: 'var(--color-success)' }}>Catalog Assistant (Online)</span>
                </div>
              </div>

              <div className="nombot-messages">
                {chatLogs.map((log, i) => (
                  <div key={i} className={`chat-bubble ${log.sender}`}>
                    {renderMarkdown(log.text)}
                  </div>
                ))}
                {askingBot && (
                  <div className="chat-bubble bot"><span className="ai-thinking"><span></span><span></span><span></span></span> Thinking...</div>
                )}
              </div>

              <div className="nombot-chips">
                <button className="nombot-chip" onClick={() => askNomBot("Give me a pipeline summary")}>📊 Pipeline Summary</button>
                <button className="nombot-chip" onClick={() => askNomBot("Show golden master catalog records")}>🏆 Golden Catalog</button>
                <button className="nombot-chip" onClick={() => askNomBot("Explain plant extension")}>💡 Plant Extension</button>
                <button className="nombot-chip" onClick={() => askNomBot("What governance rules apply to duplicate materials?")}>🔍 Dedup Rules</button>
              </div>
            </div>

            <div className="nombot-footer">
              <form className="nombot-form" onSubmit={(e) => { e.preventDefault(); askNomBot(); }}>
                <input
                  className="form-input"
                  type="text"
                  placeholder="Type your message..."
                  value={chatMessage}
                  onChange={(e) => setChatMessage(e.target.value)}
                />
                <button type="submit" className="btn btn-primary btn-sm" disabled={askingBot}>Send</button>
              </form>
            </div>
          </aside>
        )}

      </div>

      {/* Floating NomBot Toggle Button */}
      <button className="nombot-toggle-btn" onClick={() => setIsBotOpen(!isBotOpen)} title="Open NomBot Assistant">
        {isBotOpen ? <CloseIcon /> : <RobotIcon />}
      </button>

      {/* Help / Navigation Modal */}
      {showHelpModal && (
        <div className="modal-overlay" onClick={() => setShowHelpModal(false)}>
          <div className="help-modal" onClick={(e) => e.stopPropagation()}>
            <div className="help-modal-header">
              <span>NOMCAT Governance Console Navigation Guide</span>
              <button
                className="help-modal-close"
                onClick={() => setShowHelpModal(false)}
              >
                <CloseIcon />
              </button>
            </div>
            <div className="help-modal-body">
              <div className="help-section">
                <div className="help-section-icon"><DashboardIcon /></div>
                <div className="help-section-content">
                  <span className="help-section-title">Dashboard</span>
                  <span className="help-section-desc">
                    View governance analytics, total records, active taxonomies, and live catalog updates streams.
                  </span>
                </div>
              </div>

              <div className="help-section">
                <div className="help-section-icon"><CatalogIcon /></div>
                <div className="help-section-content">
                  <span className="help-section-title">Material Catalog</span>
                  <span className="help-section-desc">
                    The Golden Master database registry. Search using the header global search to instantly filter the entire catalog.
                  </span>
                </div>
              </div>

              <div className="help-section">
                <div className="help-section-icon"><WorkTrayIcon /></div>
                <div className="help-section-content">
                  <span className="help-section-title">Work Tray</span>
                  <span className="help-section-desc">
                    Staging workflow queue. Approve or Reject requests matching your active login role in sequential order.
                  </span>
                </div>
              </div>

              <div className="help-section">
                <div className="help-section-icon"><NewRequestIcon /></div>
                <div className="help-section-content">
                  <span className="help-section-title">New Request</span>
                  <span className="help-section-desc">
                    Submit a single material request. Toggle AI Smart Assist to extract attributes using Gemini NLP.
                  </span>
                </div>
              </div>

              <div className="help-section">
                <div className="help-section-icon"><BulkImportIcon /></div>
                <div className="help-section-content">
                  <span className="help-section-title">Bulk Import</span>
                  <span className="help-section-desc">
                    Import multiple records at once using Excel templates, passing automated duplicate checks.
                  </span>
                </div>
              </div>

              <div className="help-section">
                <div className="help-section-icon"><ReportingIcon /></div>
                <div className="help-section-content">
                  <span className="help-section-title">Reporting</span>
                  <span className="help-section-desc">
                    Export the staging database to CSV format for external validation or logging.
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

export default App;
