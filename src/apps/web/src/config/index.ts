export const config = {
  descope: {
    projectId: import.meta.env.VITE_DESCOPE_PROJECT_ID || 'YOUR_DESCOPE_PROJECT_ID',
    baseUrl: import.meta.env.VITE_DESCOPE_BASE_URL,
  },
  api: {
    baseUrl: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api',
  },
} as const;