<template>
  <nav v-if="totalPages > 1" aria-label="Submission navigation">
    <ul class="pagination justify-content-center">
      <!-- First Page -->
      <li class="page-item" :class="{ disabled: currentPage === 1 }">
        <button class="page-link" @click="goToPage(1)" :disabled="currentPage === 1">
          « First
        </button>
      </li>

      <!-- Previous Page -->
      <li class="page-item" :class="{ disabled: currentPage === 1 }">
        <button class="page-link" @click="goToPage(currentPage - 1)" :disabled="currentPage === 1">
          ‹ Previous
        </button>
      </li>

      <!-- Page Numbers -->
      <li
        v-for="page in visiblePages"
        :key="page"
        class="page-item"
        :class="{ active: page === currentPage }"
      >
        <button class="page-link" @click="goToPage(page)">
          {{ page }}
        </button>
      </li>

      <!-- Next Page -->
      <li class="page-item" :class="{ disabled: currentPage === totalPages }">
        <button
          class="page-link"
          @click="goToPage(currentPage + 1)"
          :disabled="currentPage === totalPages"
        >
          Next ›
        </button>
      </li>

      <!-- Last Page -->
      <li class="page-item" :class="{ disabled: currentPage === totalPages }">
        <button
          class="page-link"
          @click="goToPage(totalPages)"
          :disabled="currentPage === totalPages"
        >
          Last »
        </button>
      </li>
    </ul>

    <!-- Page Info -->
    <div class="text-center text-muted mt-2">
      Page {{ currentPage }} of {{ totalPages }} ({{ totalCount }} total submissions)
    </div>
  </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue'

// Define props
interface Props {
  currentPage: number
  totalPages: number
  totalCount: number
}

const props = defineProps<Props>()

const emit = defineEmits<{
  pageChange: [page: number]
}>()

const visiblePages = computed(() => {
  const current = props.currentPage
  const total = props.totalPages
  const range = 3 // Show 3 pages on each side of current

  let start = Math.max(1, current - range)
  let end = Math.min(total, current + range)

  if (current <= range) {
    end = Math.min(total, range * 2 + 1)
  }
  if (current >= total - range) {
    start = Math.max(1, total - range * 2)
  }

  const pages = []
  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  return pages
})

const goToPage = (page: number) => {
  if (page < 1 || page > props.totalPages) return
  emit('pageChange', page)
}
</script>

<style scoped>
.pagination {
  margin-top: 2rem;
  margin-bottom: 1rem;
}

.page-item.active .page-link {
  background-color: #0d6efd;
  border-color: #0d6efd;
}

.page-link {
  color: #0d6efd;
  cursor: pointer;
}

.page-link:hover {
  color: #0a58ca;
}
</style>
