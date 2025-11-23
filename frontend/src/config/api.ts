export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5127/api/Submissions',
  ENDPOINTS: {
    SEARCH: '/search',
    TEST_DATA: '/testData',
    ALL: '/all',
    POST: '',
  },
} as const

export const getApiUrl = (endpoint: keyof typeof API_CONFIG.ENDPOINTS) => {
  return API_CONFIG.BASE_URL + API_CONFIG.ENDPOINTS[endpoint]
}
