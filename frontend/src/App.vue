<template>
  <div class="container mt-4">
    <!-- Dashboard Header -->
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h1 class="h3">📋 Form Submissions</h1>
    </div>

    <!-- ToolBar -->
    <div class="row mb-3">
      <div class="col-md-6 position-relative">
        <input
          v-if="!isAdvancedSearchActive"
          v-model="searchQuery"
          type="text"
          class="form-control"
          :placeholder="`Search by ${defSearchField}`"
          @input="performSearch"
        />
        <!-- Loading indicator -->
        <div v-if="isSearching" class="position-absolute top-50 end-0 translate-middle-y me-3">
          <div class="spinner-border spinner-border-sm text-primary" role="status">
            <span class="visually-hidden">Searching...</span>
          </div>
        </div>
      </div>

      <div class="col-md-6 d-flex gap-2 justify-content-md-end">
        <button
          class="btn btn-outline-secondary"
          type="button"
          @click="toggleSearchMode"
          :title="isAdvancedSearchActive ? 'Switch to quick search' : 'Switch to advanced search'"
        >
          {{ isAdvancedSearchActive ? '⚡ Quick search' : '🔍 Advanced search' }}
        </button>
        <button
          class="btn btn-outline-info flex-shrink-0"
          @click="generateTestData"
          :disabled="loading"
        >
          {{ loading ? 'Generating...' : `🎲 +${randomSubmitsCount} Random Submits` }}
        </button>
        <button class="btn btn-primary flex-shrink-0" @click="openCreateModal">+ New Form</button>
      </div>
    </div>

    <!-- Main View -->
    <div class="dashboard-layout" :class="{ 'advanced-search-active': isAdvancedSearchActive }">
      <!-- Advanced Search Form -->
      <div class="advanced-search-sidebar" v-if="isAdvancedSearchActive">
        <div class="card">
          <div class="card-header bg-light">
            <h6 class="mb-0">🔍 Advanced Search</h6>
          </div>
          <div class="card-body">
            <form @submit.prevent="performAdvancedSearch">
              <!-- Name Search -->
              <div class="mb-3">
                <label class="form-label small">Name Contains</label>
                <input
                  v-model="advancedSearch.name"
                  type="text"
                  class="form-control form-control-sm"
                  placeholder="Enter name..."
                />
              </div>

              <!-- Country Filter -->
              <div class="mb-3">
                <label class="form-label small">Country</label>
                <select v-model="advancedSearch.country" class="form-select form-select-sm">
                  <option value="">Any Country</option>
                  <option value="ru">Russia</option>
                  <option value="us">United States</option>
                  <option value="de">Germany</option>
                  <option value="gb">Great Britain</option>
                </select>
              </div>

              <!-- Gender Filter -->
              <div class="mb-3">
                <label class="form-label small">Gender</label>
                <div>
                  <div class="form-check">
                    <input
                      v-model="advancedSearch.gender"
                      class="form-check-input"
                      type="radio"
                      value=""
                      id="genderAny"
                    />
                    <label class="form-check-label small" for="genderAny">Any</label>
                  </div>
                  <div class="form-check">
                    <input
                      v-model="advancedSearch.gender"
                      class="form-check-input"
                      type="radio"
                      value="male"
                      id="genderMale"
                    />
                    <label class="form-check-label small" for="genderMale">Male</label>
                  </div>
                  <div class="form-check">
                    <input
                      v-model="advancedSearch.gender"
                      class="form-check-input"
                      type="radio"
                      value="female"
                      id="genderFemale"
                    />
                    <label class="form-check-label small" for="genderFemale">Female</label>
                  </div>
                </div>
              </div>

              <!-- Birth Date Range -->
              <div class="mb-3">
                <label class="form-label small">Birth Date Range</label>
                <div class="row g-2">
                  <div class="col-6">
                    <input
                      v-model="advancedSearch.birthDateFrom"
                      type="date"
                      class="form-control form-control-sm"
                      placeholder="From"
                    />
                  </div>
                  <div class="col-6">
                    <input
                      v-model="advancedSearch.birthDateTo"
                      type="date"
                      class="form-control form-control-sm"
                      placeholder="To"
                    />
                  </div>
                </div>
              </div>

              <!-- Newsletter Filter -->
              <div class="mb-3">
                <div class="form-check">
                  <input
                    v-model="advancedSearch.newsletter"
                    class="form-check-input"
                    type="checkbox"
                    id="newsletterFilter"
                    :true-value="true"
                    :false-value="null"
                  />
                  <label class="form-check-label small" for="newsletterFilter"
                    >Subscribed to Newsletter</label
                  >
                </div>
              </div>

              <!-- Form Type Filter -->
              <div class="mb-3" v-if="availableFormTypes.length > 1">
                <label class="form-label small">Form Type</label>
                <select v-model="advancedSearch.formType" class="form-select form-select-sm">
                  <option v-for="formType in availableFormTypes" :key="formType" :value="formType">
                    {{ formType }}
                  </option>
                </select>
              </div>

              <!-- Action Buttons -->
              <div class="d-grid gap-2">
                <button type="submit" class="btn btn-primary btn-sm" :disabled="isSearching">
                  {{ isSearching ? 'Searching...' : 'Search' }}
                </button>
                <button
                  type="button"
                  class="btn btn-outline-secondary btn-sm"
                  @click="resetAdvancedSearch"
                >
                  Reset
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
      <!-- Submissions Table -->
      <div class="submissions-main">
        <div v-if="searchQuery.trim()" class="row mb-2">
          <div class="col">
            <div class="alert alert-info py-2 d-flex align-items-center">
              <span>🔍 Showing results for: "{{ searchQuery }}"</span>
              <button class="btn btn-sm btn-outline-secondary ms-auto" @click="clearSearch">
                Clear Search
              </button>
            </div>
          </div>
        </div>
        <div class="card">
          <div class="card-body">
            <table class="table table-striped">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Country</th>
                  <th>Birth Date</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="submission in submissions" :key="submission.id">
                  <td>{{ submission.formData.name }}</td>
                  <td>{{ submission.formData.country }}</td>
                  <td>{{ submission.formData.birthDate }}</td>
                  <td>
                    <button
                      class="btn btn-sm btn-outline-primary"
                      @click="openViewModal(submission)"
                    >
                      Show
                    </button>
                  </td>
                </tr>
                <tr v-if="submissions.length === 0">
                  <td colspan="4" class="text-center text-muted">
                    No submissions yet. Create your first form!
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <Pagination
            :current-page="pagination.currentPage"
            :total-pages="pagination.totalPages"
            :total-count="pagination.totalCount"
            @page-change="goToPage"
          />
        </div>
      </div>
    </div>

    <!-- Form Modal -->
    <div
      v-if="showFormModal"
      class="modal fade show d-block"
      style="background-color: rgba(0, 0, 0, 0.5)"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              {{
                modalMode === 'create'
                  ? 'Create New Form'
                  : modalMode === 'view'
                    ? 'View Submission'
                    : 'Edit Submission'
              }}
            </h5>
            <button type="button" class="btn-close" @click="showFormModal = false"></button>
          </div>
          <div class="modal-body">
            <form>
              <!-- Text Input -->
              <div class="mb-3">
                <label class="form-label">Full Name</label>
                <input
                  v-model="formData.name"
                  type="text"
                  class="form-control"
                  placeholder="Enter your name"
                  :readonly="modalMode === 'view'"
                />
              </div>

              <!-- Dropdown -->
              <div class="mb-3">
                <label class="form-label">Country</label>
                <select
                  v-model="formData.country"
                  class="form-select"
                  :disabled="modalMode === 'view'"
                >
                  <option value="">Select country</option>
                  <option value="ru">Russia</option>
                  <option value="us">United States</option>
                  <option value="de">Germany</option>
                  <option value="gb">Greate Britain</option>
                </select>
              </div>

              <!-- Date Picker -->
              <div class="mb-3">
                <label class="form-label">Birth Date</label>
                <input
                  v-model="formData.birthDate"
                  type="date"
                  class="form-control"
                  :readonly="modalMode === 'view'"
                />
              </div>

              <!-- Radio Buttons -->
              <div class="mb-3">
                <label class="form-label">Gender</label>
                <div>
                  <div class="form-check form-check-inline">
                    <input
                      v-model="formData.gender"
                      class="form-check-input"
                      type="radio"
                      value="male"
                      :disabled="modalMode === 'view'"
                    />
                    <label class="form-check-label">Male</label>
                  </div>
                  <div class="form-check form-check-inline">
                    <input
                      v-model="formData.gender"
                      class="form-check-input"
                      type="radio"
                      value="female"
                      :disabled="modalMode === 'view'"
                    />
                    <label class="form-check-label">Female</label>
                  </div>
                </div>
              </div>

              <!-- Checkbox -->
              <div class="mb-3 form-check">
                <input
                  v-model="formData.newsletter"
                  type="checkbox"
                  class="form-check-input"
                  :disabled="modalMode === 'view'"
                />
                <label class="form-check-label"> Subscribe to newsletter </label>
              </div>
            </form>
          </div>
          <div class="modal-footer">
            <button
              v-if="modalMode !== 'create'"
              type="button"
              :class="modalMode === 'edit' ? 'btn btn-warning' : 'btn btn-secondary'"
              @click="switchEditMode"
            >
              Edit
            </button>
            <button type="button" class="btn btn-secondary" @click="showFormModal = false">
              {{ modalMode === 'edit' ? 'Cancel' : 'Close' }}
            </button>
            <button type="button" class="btn btn-primary" @click="submitForm">
              {{ modalMode === 'create' ? 'Submit Form' : 'Save Changes' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import Pagination from './components/PaginationControl.vue'

interface FormData {
  name: string
  country: string
  birthDate: string
  gender: string
  newsletter: boolean
}

interface Submission {
  id: number
  formType: string
  submittedAt: string
  formData: FormData
}
interface SubmissionResponse {
  id: number
  formType: string
  submittedAt: string
  formData: string
}

interface PaginatedResult {
  items: SubmissionResponse[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

interface FormFieldFilter {
  fieldPath: string
  operator: 'contains' | 'range' | 'equals'
  value: any
  value2?: any
}

interface FormSearchRequest {
  formType?: string
  fromDate?: Date | null
  toDate?: Date | null
  fieldFilters: FormFieldFilter[]
  page: number
  pageSize: number
}

const showFormModal = ref(false)
const searchQuery = ref('')
const modalMode = ref<'create' | 'view' | 'edit'>('create')
const currentSubmissionId = ref<number | null>(null)
const loading = ref(false)
const apiUrl = 'http://localhost:5127/api/submissions'
const randomSubmitsCount = 25
const searchTimeout = ref<number | null>(null)
const isSearching = ref(false)
const availableFormTypes = ref(['ContactForm'])
const formType = ref('ContactForm')
const defSearchField = ref<string>('name')
const isAdvancedSearchActive = ref(false)

const formData = reactive<FormData>({
  name: '',
  country: '',
  birthDate: '',
  gender: '',
  newsletter: false,
})

// Sample data - we'll replace with real API calls later
const submissions = ref<Submission[]>([
  {
    id: 1,
    formType: 'ContactForm',
    submittedAt: '2024-01-15',
    formData: {
      name: 'Example Data',
      country: 'us',
      birthDate: '1990-01-01',
      gender: 'male',
      newsletter: true,
    },
  },
])

// Advanced search state
const advancedSearch = reactive({
  name: '',
  country: '',
  gender: '',
  birthDateFrom: null as Date | null,
  birthDateTo: null as Date | null,
  newsletter: null as boolean | null,
  formType: formType.value,
})

const performSearch = () => {
  if (searchTimeout.value) {
    clearTimeout(searchTimeout.value)
  }

  searchTimeout.value = setTimeout(async () => {
    pagination.currentPage = 1
    if (searchQuery.value.trim()) {
      await searchSubmissionsByDefField()
    } else {
      await loadSubmissions()
    }
  }, 500) as unknown as number
}

const performAdvancedSearch = async (resetPage: boolean = true) => {
  isSearching.value = true
  isAdvancedSearchActive.value = true

  if (resetPage) {
    pagination.currentPage = 1
  }

  try {
    const fieldFilters: FormFieldFilter[] = []

    // Name filter
    if (advancedSearch.name.trim()) {
      fieldFilters.push({
        fieldPath: 'name',
        operator: 'contains',
        value: advancedSearch.name,
      })
    }

    // Country filter
    if (advancedSearch.country) {
      fieldFilters.push({
        fieldPath: 'country',
        operator: 'equals',
        value: advancedSearch.country,
      })
    }

    // Gender filter
    if (advancedSearch.gender) {
      fieldFilters.push({
        fieldPath: 'gender',
        operator: 'equals',
        value: advancedSearch.gender,
      })
    }

    // Birth date range filter
    if (advancedSearch.birthDateFrom || advancedSearch.birthDateTo) {
      fieldFilters.push({
        fieldPath: 'birthDate',
        operator: 'range',
        value: advancedSearch.birthDateFrom,
        value2: advancedSearch.birthDateTo,
      })
    }

    // Newsletter filter
    if (advancedSearch.newsletter !== null) {
      fieldFilters.push({
        fieldPath: 'newsletter',
        operator: 'equals',
        value: advancedSearch.newsletter,
      })
    }

    const searchRequest: FormSearchRequest = {
      formType: advancedSearch.formType,
      fromDate: null,
      toDate: null,
      fieldFilters: fieldFilters,
      page: pagination.currentPage,
      pageSize: pagination.pageSize,
    }

    const response = await fetch(`${apiUrl}/search`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(searchRequest),
    })

    if (response.ok) {
      const result: PaginatedResult = await response.json()

      submissions.value = result.items.map((sub: SubmissionResponse) => ({
        id: sub.id,
        formType: sub.formType,
        submittedAt: sub.submittedAt,
        formData: JSON.parse(sub.formData),
      }))

      pagination.totalCount = result.totalCount
      pagination.totalPages = result.totalPages
    }
  } catch (error) {
    console.error('Error performing advanced search:', error)
  } finally {
    isSearching.value = false
  }
}

const resetAdvancedSearch = () => {
  Object.assign(advancedSearch, {
    name: '',
    country: '',
    gender: '',
    birthDateFrom: '',
    birthDateTo: '',
    newsletter: null,
    formType: formType.value,
  })

  searchQuery.value = ''
  pagination.currentPage = 1
  loadSubmissions()
}

const searchSubmissionsByDefField = async () => {
  isSearching.value = true
  try {
    const searchRequest: FormSearchRequest = {
      formType: formType.value,
      fromDate: null,
      toDate: null,
      fieldFilters: [
        {
          fieldPath: defSearchField.value,
          operator: 'contains',
          value: searchQuery.value,
        },
      ],
      page: pagination.currentPage,
      pageSize: pagination.pageSize,
    }

    const response = await fetch(`${apiUrl}/search`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(searchRequest),
    })

    if (response.ok) {
      const result: PaginatedResult = await response.json()

      submissions.value = result.items.map((sub: SubmissionResponse) => ({
        id: sub.id,
        formType: sub.formType,
        submittedAt: sub.submittedAt,
        formData: JSON.parse(sub.formData),
      }))

      pagination.totalCount = result.totalCount
      pagination.totalPages = result.totalPages
    }
  } catch (error) {
    console.error('Error searching submissions:', error)
  } finally {
    isSearching.value = false
  }
}

const generateTestData = async () => {
  loading.value = true
  try {
    const response = await fetch(`${apiUrl}/testData?count=${randomSubmitsCount}`, {
      method: 'POST',
    })
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
    searchQuery.value = ''
    pagination.currentPage = 1
    await loadSubmissions()
  } catch (error) {
    console.error('Error generating test data:', error)
    alert('Error generating test data. Make sure the backend is running!')
  } finally {
    loading.value = false
  }
}

const pagination = reactive({
  currentPage: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 0,
})

const openCreateModal = () => {
  modalMode.value = 'create'
  currentSubmissionId.value = null
  resetForm()
  showFormModal.value = true
}

const openViewModal = (submission: Submission) => {
  modalMode.value = 'view'
  currentSubmissionId.value = submission.id
  Object.assign(formData, submission.formData)
  showFormModal.value = true
}

const switchEditMode = () => {
  if (modalMode.value === 'view') {
    modalMode.value = 'edit'
  } else {
    modalMode.value = 'view'
    Object.assign(
      formData,
      submissions.value.find((s) => s.id === currentSubmissionId.value)!.formData,
    )
  }
}

const resetForm = () => {
  Object.assign(formData, {
    name: '',
    country: '',
    birthDate: '',
    gender: '',
    newsletter: false,
  })
}

// Method to handle form submission
const submitForm = async () => {
  try {
    const submissionDto = {
      formType: 'ContactForm',
      submittedAt: new Date().toISOString(),
      formData: JSON.stringify(formData),
    }

    const response = await fetch(apiUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(submissionDto),
    })

    if (response.ok) {
      const result = await response.json()
      console.log('Form submitted with ID:', result.id)

      // Close modal and reset form
      showFormModal.value = false
      resetForm()
      clearSearch()
    }
  } catch (error) {
    console.error('Error submitting form:', error)
  }
}

const clearSearch = () => {
  searchQuery.value = ''
  pagination.currentPage = 1
  loadSubmissions()
}

const toggleSearchMode = () => {
  if (isAdvancedSearchActive.value) {
    resetAdvancedSearch()
    isAdvancedSearchActive.value = false
  } else {
    if (searchQuery.value.trim()) {
      advancedSearch[defSearchField.value] = searchQuery.value //simple solution for fast dev
    }
    isAdvancedSearchActive.value = true
    searchQuery.value = ''
  }
}

// Method to change pages
const goToPage = (page: number) => {
  if (page < 1 || page > pagination.totalPages) return
  pagination.currentPage = page

  if (isAdvancedSearchActive.value) {
    performAdvancedSearch(false)
  }
  if (searchQuery.value.trim()) {
    searchSubmissionsByDefField()
  } else {
    loadSubmissions()
  }
}

const loadSubmissions = async () => {
  loading.value = true
  try {
    const response = await fetch(
      `${apiUrl}/all/ContactForm?page=${pagination.currentPage}&pageSize=${pagination.pageSize}`,
    )
    if (response.ok) {
      const result: PaginatedResult = await response.json()

      submissions.value = result.items.map((sub: SubmissionResponse) => ({
        id: sub.id,
        formType: sub.formType,
        submittedAt: sub.submittedAt,
        formData: JSON.parse(sub.formData),
      }))

      pagination.totalCount = result.totalCount
      pagination.totalPages = result.totalPages
    }
  } catch (error) {
    console.error('Error loading submissions:', error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadSubmissions()
})
</script>

<style scoped>
.modal-backdrop {
  background-color: rgba(0, 0, 0, 0.5);
}
.table th {
  border-top: none;
  font-weight: 600;
}
.card {
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}
.search-loading {
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
}
.alert {
  border-radius: 0.375rem;
}
.dashboard-layout {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
}
.dashboard-layout.advanced-search-active {
  grid-template-columns: 300px 1fr;
}
.advanced-search-sidebar {
}
.submissions-main {
  min-width: 0;
}
.table-responsive {
  overflow-x: auto;
}
</style>
