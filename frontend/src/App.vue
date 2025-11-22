<template>
  <div class="container mt-4">
    <!-- Dashboard Header -->
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h1 class="h3">📋 Form Submissions</h1>
      <button class="btn btn-primary" @click="openCreateModal">+ New Form</button>
      <button class="btn btn-outline-info" @click="generateTestData" :disabled="loading">
        {{ loading ? 'Generating...' : '🎲 Generate Test Data' }}
      </button>
    </div>

    <!-- Search Bar -->
    <div class="row mb-3">
      <div class="col-md-6">
        <input
          v-model="searchQuery"
          type="text"
          class="form-control"
          placeholder="Search submissions..."
        />
      </div>
    </div>

    <!-- Submissions Table -->
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
            <tr v-for="submission in filteredSubmissions" :key="submission.id">
              <td>{{ submission.formData.name }}</td>
              <td>{{ submission.formData.country }}</td>
              <td>{{ submission.formData.birthDate }}</td>
              <td>
                <button class="btn btn-sm btn-outline-primary" @click="openViewModal(submission)">
                  Show
                </button>
              </td>
            </tr>
            <tr v-if="filteredSubmissions.length === 0">
              <td colspan="4" class="text-center text-muted">
                No submissions yet. Create your first form!
              </td>
            </tr>
          </tbody>
        </table>
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
import { ref, computed, reactive, onMounted } from 'vue'

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

const showFormModal = ref(false)
const searchQuery = ref('')
const modalMode = ref<'create' | 'view' | 'edit'>('create')
const currentSubmissionId = ref<number | null>(null)
const loading = ref(false)
const apiUrl = 'http://localhost:5127/api/submissions'

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

const generateTestData = async () => {
  loading.value = true
  try {
    const response = await fetch(`${apiUrl}/testData`, {
      method: 'POST',
    })

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }

    const newSubmissions = await response.json()

    // Transform backend data to match our frontend structure
    const transformedSubmissions = newSubmissions.map((sub: any) => ({
      id: sub.id,
      formType: sub.formType,
      submittedAt: sub.submittedAt,
      formData: JSON.parse(sub.formData),
    }))

    submissions.value = [...submissions.value, ...transformedSubmissions]

    alert(`Successfully generated ${transformedSubmissions.length} test submissions!`)
  } catch (error) {
    console.error('Error generating test data:', error)
    alert('Error generating test data. Make sure the backend is running!')
  } finally {
    loading.value = false
  }
}

const filteredSubmissions = computed(() => {
  if (!searchQuery.value) return submissions.value
  return submissions.value.filter((sub) =>
    sub.formData.name.toLowerCase().includes(searchQuery.value.toLowerCase()),
  )
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

const closeModal = () => {
  showFormModal.value = false
  resetForm()
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

      // Refresh the submissions list
      await loadSubmissions()
    }
  } catch (error) {
    console.error('Error submitting form:', error)
  }
}

const loadSubmissions = async () => {
  try {
    const response = await fetch(`${apiUrl}/all/ContactForm`)
    if (response.ok) {
      const backendSubmissions = await response.json()

      submissions.value = backendSubmissions.map((sub: any) => ({
        id: sub.id,
        formType: sub.formType,
        submittedAt: sub.submittedAt,
        formData: JSON.parse(sub.formData),
      }))
    }
  } catch (error) {
    console.error('Error loading submissions:', error)
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
</style>
