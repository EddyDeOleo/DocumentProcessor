import axios from 'axios';
import type { DocumentDetailsDto, ExtractedDataDto } from '../types/document';

const API_BASE_URL = 'http://localhost:5147/api/v1/documents';

const api = axios.create({
    baseURL: API_BASE_URL,
});

export const documentApi = {
    upload: async (file: File): Promise<DocumentDetailsDto> => {
        const formData = new FormData();
        formData.append('file', file);

        const response = await api.post<DocumentDetailsDto>('/upload', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
        return response.data;
    },

    getAll: async (pageNumber = 1, pageSize = 10): Promise<DocumentDetailsDto[]> => {
        const response = await api.get<DocumentDetailsDto[]>(`?pageNumber=${pageNumber}&pageSize=${pageSize}`);
        return response.data;
    },

    getById: async (id: string): Promise<DocumentDetailsDto> => {
        const response = await api.get<DocumentDetailsDto>(`/${id}`);
        return response.data;
    },

    getStatus: async (id: string): Promise<string> => {
        const response = await api.get<string>(`/${id}/status`);
        return response.data;
    },

    getExtractedData: async (id: string): Promise<ExtractedDataDto> => {
        const response = await api.get<ExtractedDataDto>(`/${id}/extracted-data`);
        return response.data;
    },

    delete: async (id: string): Promise<void> => {
        await api.delete(`/${id}`);
    },
};