// wwwroot/js/Settings.js

/**
 * Initializes the Settings page logic
 * @param {Object} config - Contains inventoryId, canEdit, and userId
 */
function initSettingsPage(config) {
    const { inventoryId, canEdit, userId } = config;

    // Tab switching logic
    const itemsTab = document.getElementById('items-tab');
    if (itemsTab) {
        itemsTab.addEventListener('shown.bs.tab', function () {
            loadItems(inventoryId);
            clearDiscussionPolling();
        });
    }

    const discussionTab = document.getElementById('discussion-tab');
    if (discussionTab) {
        discussionTab.addEventListener('shown.bs.tab', function () {
            loadDiscussion(inventoryId, 1);
        });
    }

    const statisticsTab = document.getElementById('statistics-tab');
    if (statisticsTab) {
        statisticsTab.addEventListener('shown.bs.tab', function () {
            loadStatistics(inventoryId);
            clearDiscussionPolling();
        });
    }

    // Initial view load
    loadItems(inventoryId);

    // Event delegation for Discussion
    const discussionContainer = document.getElementById('discussionContainer');
    if (discussionContainer) {
        discussionContainer.addEventListener('click', async function (e) {      
            const pageBtn = e.target.closest('.disc-page-btn');
            if (pageBtn) {
                e.preventDefault();
                await loadDiscussion(inventoryId, parseInt(pageBtn.dataset.page, 10));
                return;
            }

            const deleteBtn = e.target.closest('[data-disc-delete] button[type="submit"]');
            if (deleteBtn) {
                e.preventDefault();
                if (!confirm('Delete this comment?')) return;
                const form = deleteBtn.closest('form[data-disc-delete]');
                const data = new FormData(form);
                const page = parseInt(data.get('returnPage'), 10) || 1;
                await fetch(form.action, { method: 'POST', body: data });
                await loadDiscussion(inventoryId, page);
            }
        });

        discussionContainer.addEventListener('submit', async function (e) {
            const postForm = e.target.closest('form[data-disc-post]');
            if (!postForm) return;
            e.preventDefault();
            if (!postForm.checkValidity()) {
                postForm.classList.add('was-validated');
                return;
            }
            const data = new FormData(postForm);
            await fetch(postForm.action, { method: 'POST', body: data });
            await loadDiscussion(inventoryId, 1);
        });
    }
}

function clearDiscussionPolling() {
    if (window.discussionPollInterval) {
        clearInterval(window.discussionPollInterval);
    }
}

// Clean up on page unload/navigation
window.addEventListener('beforeunload', function() {
    clearDiscussionPolling();
});

async function loadItems(inventoryId) {
    try {
        const response = await fetch(`/Inventories/ItemsPartial/${inventoryId}`);
        document.getElementById('itemsContainer').innerHTML = await response.text();
    } catch (error) {
        console.error('Error loading items:', error);
    }
}

async function loadDiscussion(inventoryId, page = 1) {
    try {
        const response = await fetch(`/Inventories/DiscussionPartial/${inventoryId}?page=${page}`);
        document.getElementById('discussionContainer').innerHTML = await response.text();
    } catch (error) {
        console.error('Error loading discussion:', error);
    }
}

async function loadStatistics(inventoryId) {
    try {
        const response = await fetch(`/Inventories/StatisticsPartial/${inventoryId}`);
        document.getElementById('statisticsContainer').innerHTML = await response.text();
    } catch (error) {
        console.error('Error loading statistics:', error);
    }
}