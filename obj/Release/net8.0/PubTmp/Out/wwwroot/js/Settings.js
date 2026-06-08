

function initInventorySettings(config) {
    const { inventoryId, canEdit, userId } = config;

    window.inventoryId = parseInt(inventoryId, 10);
    window.canEdit = canEdit === 'True' || canEdit === true;
    window.userId = userId;

    const itemsTab = document.getElementById('items-tab');
    if (itemsTab) {
        itemsTab.addEventListener('shown.bs.tab', function () {
            loadInventoryItems(inventoryId);
            clearDiscussionPolling();
        });
    }

    const discussionTab = document.getElementById('discussion-tab');
    if (discussionTab) {
        discussionTab.addEventListener('shown.bs.tab', function () {
            loadInventoryDiscussion(inventoryId, 1);
        });
    }

    const statisticsTab = document.getElementById('statistics-tab');
    if (statisticsTab) {
        statisticsTab.addEventListener('shown.bs.tab', function () {
            loadInventoryStatistics(inventoryId);
            clearDiscussionPolling();
        });
    }

    loadInventoryItems(inventoryId);

    try {
            if (typeof window.initInventoryFields === 'function') {
            const fieldsPane = document.getElementById('fields');
            window.initInventoryFields(fieldsPane || document, window.inventoryId);
        }
    } catch (err) {
        console.error('initFields error:', err);
    }

    attachVisibilityAutoSave();

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

function attachVisibilityAutoSave() {
    const radios = document.querySelectorAll('#access input[name="visibility"], input[name="visibility"]');
    if (!radios || radios.length === 0) return;

    radios.forEach(radio => {
        radio.addEventListener('change', async (e) => {
            const selected = document.querySelector('input[name="visibility"]:checked');
            if (!selected) return;

            const visibilityValue = selected.value;
            const inventoryId = window.inventoryId || parseInt(document.querySelector('input[name="inventoryId"]')?.value || 0, 10);
            if (!inventoryId) return;

            const badge = document.querySelector('.badge.bg-success');
            if (badge) badge.textContent = 'Saving...';

            try {
                const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                const formData = new FormData();
                formData.append('id', inventoryId);
                formData.append('visibility', visibilityValue);
                if (tokenInput) formData.append('__RequestVerificationToken', tokenInput.value);

                const resp = await fetch('/Inventories/SetVisibility', {
                    method: 'POST',
                    body: formData,
                    credentials: 'same-origin'
                });

                const json = await resp.json();
                if (json && json.success) {
                    if (badge) badge.textContent = 'All changes saved';
                } else {
                    if (badge) badge.textContent = 'Save failed';
                    console.error('SetVisibility failed', json);
                }
            } catch (err) {
                console.error('Error saving visibility:', err);
                const badge = document.querySelector('.badge.bg-success');
                if (badge) badge.textContent = 'Save failed';
            } finally {
                setTimeout(() => {
                    const badge = document.querySelector('.badge.bg-success');
                    if (badge && badge.textContent !== 'All changes saved') {
                        badge.textContent = 'All changes saved';
                    }
                }, 1500);
            }
        });
    });
}

async function loadDiscussion(inventoryId, page = 1) {
    try {
        const response = await fetch(`/Inventories/DiscussionPartial/${inventoryId}?page=${page}`);
        document.getElementById('discussionContainer').innerHTML = await response.text();
    } catch (error) {
        console.error('Error loading discussion:', error);
    }
} 

function clearDiscussionPolling() {
    if (window.discussionPollInterval) {
        clearInterval(window.discussionPollInterval);
    }
}

window.addEventListener('beforeunload', function() {
    clearDiscussionPolling();
});

async function loadInventoryItems(inventoryId) {
    try {
        const response = await fetch(`/Inventories/ItemsPartial/${inventoryId}`, { credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        document.getElementById('itemsContainer').innerHTML = await response.text();
    } catch (error) {
        console.error('Error loading items:', error);
    }
}

async function loadInventoryDiscussion(inventoryId, page = 1) {
    try {
        const response = await fetch(`/Inventories/DiscussionPartial/${inventoryId}?page=${page}`, { credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        document.getElementById('discussionContainer').innerHTML = await response.text();
    } catch (error) {
        console.error('Error loading discussion:', error);
    }
}

async function loadInventoryStatistics(inventoryId) {
    try {
        const response = await fetch(`/Inventories/StatisticsPartial/${inventoryId}`, { credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        document.getElementById('statisticsContainer').innerHTML = await response.text();
    } catch (error) {
        console.error('Error loading statistics:', error);
    }
}
