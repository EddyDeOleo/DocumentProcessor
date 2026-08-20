import { useState, useEffect } from 'react';
import { Routes, Route, Link, useNavigate, useParams } from 'react-router-dom';
import { documentApi } from './api/documentApi';
import type { DocumentDetailsDto, ExtractedDataDto } from './types/document';
import {
    FileText, Upload, Trash2, Eye, RefreshCw,
    CheckCircle2, Clock, AlertCircle, Loader2, ArrowLeft, PlusCircle, Database, AlertTriangle
} from 'lucide-react';

// Helper to reliably retrieve ID regardless of C# Naming Convention
const getId = (doc: DocumentDetailsDto): string => doc.documentId || doc.id || '';

// ==========================================
// 1. NAVIGATION BAR
// ==========================================
function Navbar() {
    return (
        <header className="bg-slate-900 border-b border-slate-800 sticky top-0 z-50">
            <div className="max-w-7xl mx-auto px-6 h-16 flex items-center justify-between">
                <Link to="/" className="flex items-center gap-3 font-bold text-lg text-white">
                    <div className="p-2 bg-indigo-600 rounded-lg text-white">
                        <FileText className="w-5 h-5" />
                    </div>
                    <span>Document Processor <span className="text-indigo-400">AI</span></span>
                </Link>
                <nav className="flex items-center gap-4">
                    <Link
                        to="/"
                        className="text-sm font-medium text-slate-300 hover:text-white px-3 py-2 rounded-lg hover:bg-slate-800 transition-colors"
                    >
                        All Documents
                    </Link>
                    <Link
                        to="/upload"
                        className="text-sm font-medium bg-indigo-600 hover:bg-indigo-500 text-white px-4 py-2 rounded-lg transition-colors flex items-center gap-2"
                    >
                        <PlusCircle className="w-4 h-4" />
                        Upload PDF
                    </Link>
                </nav>
            </div>
        </header>
    );
}

// ==========================================
// 2. DOCUMENT LIST VIEW
// ==========================================
function DocumentListView() {
    const [documents, setDocuments] = useState<DocumentDetailsDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Custom delete modal state
    const [docToDelete, setDocToDelete] = useState<DocumentDetailsDto | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);

    const navigate = useNavigate();

    const fetchDocuments = async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await documentApi.getAll();
            setDocuments(data);
        } catch (err: any) {
            setError(err.message || 'Failed to fetch documents.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchDocuments();
    }, []);

    const openDeleteModal = (doc: DocumentDetailsDto, e: React.MouseEvent) => {
        e.stopPropagation();
        setDocToDelete(doc);
    };

    const handleConfirmDelete = async () => {
        if (!docToDelete) return;
        const docId = getId(docToDelete);
        if (!docId) return;

        setIsDeleting(true);
        try {
            await documentApi.delete(docId);
            setDocuments((prev) => prev.filter((d) => getId(d) !== docId));
            setDocToDelete(null);
        } catch {
            alert('Failed to delete document.');
        } finally {
            setIsDeleting(false);
        }
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold text-white">Processed Documents</h1>
                    <p className="text-slate-400 text-sm">Manage and review your AI-extracted PDF files</p>
                </div>
                <button
                    onClick={fetchDocuments}
                    className="p-2.5 bg-slate-800 hover:bg-slate-700 text-slate-300 rounded-lg transition-colors border border-slate-700 flex items-center gap-2 text-sm"
                >
                    <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
                    Refresh
                </button>
            </div>

            {error && (
                <div className="bg-red-500/10 border border-red-500/30 text-red-400 p-4 rounded-xl flex items-center gap-3">
                    <AlertCircle className="w-5 h-5 shrink-0" />
                    <span className="text-sm">{error}</span>
                </div>
            )}

            {loading ? (
                <div className="flex flex-col items-center justify-center h-64 text-slate-400">
                    <Loader2 className="w-8 h-8 animate-spin text-indigo-500 mb-2" />
                    <p className="text-sm">Loading documents...</p>
                </div>
            ) : documents.length === 0 ? (
                <div className="bg-slate-800/50 border border-slate-700/60 rounded-2xl p-12 text-center">
                    <Database className="w-12 h-12 text-slate-500 mx-auto mb-3" />
                    <h3 className="text-lg font-semibold text-white mb-1">No documents found</h3>
                    <p className="text-slate-400 text-sm mb-6">Upload your first PDF document to start AI data extraction.</p>
                    <Link
                        to="/upload"
                        className="bg-indigo-600 hover:bg-indigo-500 text-white font-medium px-5 py-2.5 rounded-lg transition-colors inline-flex items-center gap-2 text-sm"
                    >
                        <Upload className="w-4 h-4" />
                        Upload Document
                    </Link>
                </div>
            ) : (
                <div className="bg-slate-800/80 border border-slate-700 rounded-xl overflow-hidden shadow-xl">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="border-b border-slate-700 bg-slate-900/50 text-slate-400 text-xs uppercase font-semibold">
                                <th className="py-4 px-6">File Name</th>
                                <th className="py-4 px-6">Status</th>
                                <th className="py-4 px-6">Uploaded At</th>
                                <th className="py-4 px-6 text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-700/50 text-sm text-slate-200">
                            {documents.map((doc) => {
                                const docId = getId(doc);
                                return (
                                    <tr
                                        key={docId || doc.fileName}
                                        onClick={() => docId && navigate(`/documents/${docId}`)}
                                        className="hover:bg-slate-700/40 cursor-pointer transition-colors"
                                    >
                                        <td className="py-4 px-6 font-medium text-white flex items-center gap-3">
                                            <FileText className="w-5 h-5 text-indigo-400 shrink-0" />
                                            <span className="truncate max-w-xs">{doc.fileName}</span>
                                        </td>
                                        <td className="py-4 px-6">
                                            <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium ${doc.status === 'Completed' ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' :
                                                    doc.status === 'Failed' ? 'bg-red-500/10 text-red-400 border border-red-500/20' :
                                                        'bg-amber-500/10 text-amber-400 border border-amber-500/20'
                                                }`}>
                                                {doc.status === 'Completed' ? <CheckCircle2 className="w-3.5 h-3.5" /> : <Clock className="w-3.5 h-3.5" />}
                                                {doc.status}
                                            </span>
                                        </td>
                                        <td className="py-4 px-6 text-slate-400 text-xs">
                                            {new Date(doc.uploadedAt).toLocaleString()}
                                        </td>
                                        <td className="py-4 px-6 text-right">
                                            <div className="flex items-center justify-end gap-2" onClick={(e) => e.stopPropagation()}>
                                                <button
                                                    onClick={() => docId && navigate(`/documents/${docId}`)}
                                                    className="p-2 hover:bg-slate-600/50 text-slate-300 hover:text-white rounded-lg transition-colors"
                                                    title="View Extracted Data"
                                                >
                                                    <Eye className="w-4 h-4" />
                                                </button>
                                                <button
                                                    onClick={(e) => openDeleteModal(doc, e)}
                                                    className="p-2 hover:bg-red-500/20 text-slate-400 hover:text-red-400 rounded-lg transition-colors"
                                                    title="Delete Document"
                                                >
                                                    <Trash2 className="w-4 h-4" />
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                </div>
            )}

            {/* CUSTOM DELETE CONFIRMATION MODAL */}
            {docToDelete && (
                <div className="fixed inset-0 bg-slate-950/70 backdrop-blur-sm z-50 flex items-center justify-center p-4">
                    <div className="bg-slate-900 border border-slate-800 rounded-2xl max-w-md w-full p-6 space-y-5 shadow-2xl animate-in fade-in zoom-in-95 duration-150">
                        <div className="flex items-center gap-4">
                            <div className="p-3 bg-red-500/10 text-red-400 rounded-xl border border-red-500/20 shrink-0">
                                <AlertTriangle className="w-6 h-6" />
                            </div>
                            <div>
                                <h3 className="text-lg font-bold text-white">Delete Document?</h3>
                                <p className="text-xs text-slate-400 mt-0.5">This action cannot be undone.</p>
                            </div>
                        </div>

                        <div className="bg-slate-950/60 p-3.5 rounded-xl border border-slate-800">
                            <span className="text-xs text-slate-500 block mb-1">Selected file:</span>
                            <p className="text-sm text-slate-200 font-medium truncate font-mono">
                                {docToDelete.fileName}
                            </p>
                        </div>

                        <div className="flex items-center justify-end gap-3 pt-2">
                            <button
                                type="button"
                                onClick={() => setDocToDelete(null)}
                                disabled={isDeleting}
                                className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-slate-300 text-sm font-medium rounded-lg border border-slate-700 transition-colors disabled:opacity-50"
                            >
                                Cancel
                            </button>
                            <button
                                type="button"
                                onClick={handleConfirmDelete}
                                disabled={isDeleting}
                                className="px-4 py-2 bg-red-600 hover:bg-red-500 text-white text-sm font-semibold rounded-lg transition-colors flex items-center gap-2 disabled:opacity-50 shadow-lg shadow-red-600/20"
                            >
                                {isDeleting ? (
                                    <>
                                        <Loader2 className="w-4 h-4 animate-spin" />
                                        <span>Deleting...</span>
                                    </>
                                ) : (
                                    <>
                                        <Trash2 className="w-4 h-4" />
                                        <span>Yes, Delete</span>
                                    </>
                                )}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

// ==========================================
// 3. UPLOAD VIEW
// ==========================================
function UploadView() {
    const [file, setFile] = useState<File | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const handleUpload = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!file) return;

        setLoading(true);
        setError(null);

        try {
            const doc = await documentApi.upload(file);
            const docId = getId(doc);
            if (docId) {
                navigate(`/documents/${docId}`);
            } else {
                navigate('/');
            }
        } catch (err: any) {
            setError(err.response?.data?.detail || err.message || 'Error processing document.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="max-w-xl mx-auto space-y-6">
            <div className="flex items-center gap-4">
                <button
                    onClick={() => navigate('/')}
                    className="p-2 bg-slate-800 text-slate-400 hover:text-white rounded-lg border border-slate-700 transition-colors"
                >
                    <ArrowLeft className="w-5 h-5" />
                </button>
                <div>
                    <h1 className="text-2xl font-bold text-white">Upload Document</h1>
                    <p className="text-slate-400 text-sm">Select a PDF invoice, receipt, or contract</p>
                </div>
            </div>

            <form onSubmit={handleUpload} className="bg-slate-800 border border-slate-700 rounded-2xl p-8 shadow-xl space-y-6">
                <div className="border-2 border-dashed border-slate-600 hover:border-indigo-500 transition-colors rounded-xl p-8 text-center bg-slate-900/40">
                    <Upload className="w-12 h-12 text-slate-400 mx-auto mb-3" />
                    <label className="cursor-pointer bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors inline-block mb-2">
                        Select PDF File
                        <input
                            type="file"
                            accept=".pdf"
                            className="hidden"
                            onChange={(e) => setFile(e.target.files?.[0] || null)}
                        />
                    </label>
                    {file ? (
                        <p className="text-sm text-indigo-300 font-medium mt-2 truncate">
                            Ready: <span className="underline">{file.name}</span>
                        </p>
                    ) : (
                        <p className="text-xs text-slate-400 mt-1">PDF files up to 10MB supported</p>
                    )}
                </div>

                {error && (
                    <div className="bg-red-500/10 border border-red-500/30 text-red-400 p-4 rounded-xl flex items-center gap-3">
                        <AlertCircle className="w-5 h-5 shrink-0" />
                        <span className="text-sm">{error}</span>
                    </div>
                )}

                <button
                    type="submit"
                    disabled={!file || loading}
                    className="w-full bg-indigo-600 hover:bg-indigo-500 disabled:bg-slate-700 disabled:text-slate-500 text-white font-bold py-3 rounded-xl transition-all flex items-center justify-center gap-2"
                >
                    {loading ? (
                        <>
                            <Loader2 className="w-5 h-5 animate-spin" />
                            <span>Analyzing with Gemini AI...</span>
                        </>
                    ) : (
                        <span>Start Processing</span>
                    )}
                </button>
            </form>
        </div>
    );
}

// ==========================================
// 4. DOCUMENT DETAIL VIEW
// ==========================================
function DocumentDetailView() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [doc, setDoc] = useState<DocumentDetailsDto | null>(null);
    const [extractedData, setExtractedData] = useState<ExtractedDataDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!id || id === 'undefined') {
            setError('Invalid Document ID');
            setLoading(false);
            return;
        }

        const loadData = async () => {
            setLoading(true);
            setError(null);
            try {
                const docDetails = await documentApi.getById(id);
                setDoc(docDetails);

                // Try standalone extracted data endpoint, fallback to embedded extractedData
                try {
                    const data = await documentApi.getExtractedData(id);
                    setExtractedData(data);
                } catch {
                    setExtractedData(docDetails.extractedData || null);
                }
            } catch (err: any) {
                setError(err.message || 'Failed to load document details.');
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [id]);

    if (loading) {
        return (
            <div className="flex flex-col items-center justify-center h-64 text-slate-400">
                <Loader2 className="w-8 h-8 animate-spin text-indigo-500 mb-2" />
                <p className="text-sm">Retrieving document details...</p>
            </div>
        );
    }

    if (error || !doc) {
        return (
            <div className="max-w-xl mx-auto space-y-4">
                <div className="bg-red-500/10 border border-red-500/30 text-red-400 p-4 rounded-xl flex items-center gap-3">
                    <AlertCircle className="w-5 h-5" />
                    <span className="text-sm">{error || 'Document not found.'}</span>
                </div>
                <button onClick={() => navigate('/')} className="text-indigo-400 hover:underline text-sm font-medium">
                    &larr; Back to documents
                </button>
            </div>
        );
    }

    const docId = getId(doc);

    return (
        <div className="space-y-6">
            <div className="flex items-center gap-4">
                <button
                    onClick={() => navigate('/')}
                    className="p-2 bg-slate-800 text-slate-400 hover:text-white rounded-lg border border-slate-700 transition-colors"
                >
                    <ArrowLeft className="w-5 h-5" />
                </button>
                <div>
                    <h1 className="text-2xl font-bold text-white truncate max-w-lg">{doc.fileName}</h1>
                    <p className="text-slate-400 text-xs">ID: {docId}</p>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div className="bg-slate-800 border border-slate-700 rounded-2xl p-6 space-y-4 h-fit">
                    <h2 className="text-sm font-semibold text-slate-300 uppercase tracking-wider">Document Summary</h2>
                    <div className="space-y-3 text-sm">
                        <div>
                            <span className="text-slate-400 block text-xs">Status</span>
                            <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 mt-1">
                                <CheckCircle2 className="w-3.5 h-3.5" />
                                {doc.status}
                            </span>
                        </div>
                        <div>
                            <span className="text-slate-400 block text-xs">Uploaded At</span>
                            <span className="text-slate-200 font-medium">{new Date(doc.uploadedAt).toLocaleString()}</span>
                        </div>
                        {extractedData?.confidenceScore !== undefined && (
                            <div>
                                <span className="text-slate-400 block text-xs">AI Confidence Score</span>
                                <span className="text-indigo-400 font-mono font-bold">
                                    {(extractedData.confidenceScore * 100).toFixed(0)}%
                                </span>
                            </div>
                        )}
                    </div>
                </div>

                <div className="lg:col-span-2 bg-slate-800 border border-slate-700 rounded-2xl p-6 space-y-4">
                    <h2 className="text-sm font-semibold text-slate-300 uppercase tracking-wider flex items-center justify-between">
                        <span>Extracted Data (Gemini JSON)</span>
                        <span className="text-xs text-indigo-400 font-normal bg-indigo-500/10 px-2.5 py-1 rounded-md border border-indigo-500/20">
                            {extractedData?.documentType || 'Structured Output'}
                        </span>
                    </h2>

                    <pre className="bg-slate-950 p-5 rounded-xl text-emerald-400 font-mono text-xs overflow-x-auto border border-slate-900 leading-relaxed max-h-[500px]">
                        {JSON.stringify(extractedData || doc, null, 2)}
                    </pre>
                </div>
            </div>
        </div>
    );
}

// ==========================================
// MAIN APP ROUTER
// ==========================================
export default function App() {
    return (
        <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col font-sans">
            <Navbar />
            <main className="max-w-7xl w-full mx-auto px-6 py-8 flex-1">
                <Routes>
                    <Route path="/" element={<DocumentListView />} />
                    <Route path="/upload" element={<UploadView />} />
                    <Route path="/documents/:id" element={<DocumentDetailView />} />
                </Routes>
            </main>
        </div>
    );
}