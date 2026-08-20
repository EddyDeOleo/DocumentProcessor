export type DocumentStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed';
export type DocumentType = 'Invoice' | 'Receipt' | 'Contract' | 'TaxForm' | 'Unknown';

export interface ExtractedDataDto {
    documentType?: DocumentType;
    vendorName?: string;
    taxId?: string;
    totalAmount?: number;
    currency?: string;
    issueDate?: string;
    confidenceScore?: number;
    rawJsonResponse?: string;
}

export interface DocumentDetailsDto {
    id?: string;
    documentId?: string;
    fileName: string;
    storageUrl?: string;
    status: DocumentStatus;
    uploadedAt: string;
    processedAt?: string;
    extractedData?: ExtractedDataDto;
}