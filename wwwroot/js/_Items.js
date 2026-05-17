(function () {
    'use strict';

    const INV_ID = "@inventory.Id";
    const USER_ID = '@userId.Replace("'; ","; '")';
    const CAN_EDIT = '@(canEdit ? "true" : "false")';
    const IS_AUTH = '@(isAuthenticated ? "true" : "false")';

    let bsModal = null;
    let editingItemId = null; 

    /* ------------------------------------------------------------------ */
    /* Boot                                                                  */
    /* ------------------------------------------------------------------ */
    document.addEventListener('DOMContentLoaded', () => {
        const modalEl = document.getElementById('itemModal');
        if (modalEl) bsModal = new bootstrap.Modal(modalEl);

        bindAddButtons();
        bindSave();
        bindCheckboxes();
        bindToolbox();
        loadAllLikes();
        bindLikeButtons();
    });

    /* ------------------------------------------------------------------ */
    /* Add / Edit modal                                                      */
    /* ------------------------------------------------------------------ */
    function bindAddButtons() {
        document.getElementById('addItemBtn')?.addEventListener('click', openAddModal);
        document.getElementById('addFirstItem')?.addEventListener('click', openAddModal);
    }

    function openAddModal() {
        editingItemId = null;
        resetForm();
        document.getElementById('itemModalLabel').textContent = 'Add Item';
        document.getElementById('saveBtn').textContent = 'Save Item';
        bsModal?.show();
    }

    function openEditModal(itemId) {
        editingItemId = itemId;
        document.getElementById('itemModalLabel').textContent = 'Edit Item';
        document.getElementById('saveBtn').textContent = 'Update Item';
        fetchItemForEdit(itemId);
    }

    function resetForm() {
        document.querySelectorAll('#itemFormFields input, #itemFormFields textarea, #itemFormFields select').forEach(el => {
            if (el.type === 'checkbox') el.checked = false;
            else el.value = '';
        });
    }

    async function fetchItemForEdit(itemId) {
        try {
            const resp = await fetch(`/api/items/${itemId}`);
            if (!resp.ok) throw new Error('Failed to fetch item');
            const item = await resp.json();
            resetForm();

            // Map custom field values to form inputs by field name
            const fieldMap = {
                customString1Value: 'CustomString1Value',
                customString2Value: 'CustomString2Value',
                customString3Value: 'CustomString3Value',
                customText1Value: 'CustomText1Value',
                customText2Value: 'CustomText2Value',
                customText3Value: 'CustomText3Value',
                customNumber1Value: 'CustomNumber1Value',
                customNumber2Value: 'CustomNumber2Value',
                customNumber3Value: 'CustomNumber3Value',
                customBool1Value: 'CustomBool1Value',
                customBool2Value: 'CustomBool2Value',
                customBool3Value: 'CustomBool3Value',
                customLink1Value: 'CustomLink1Value',
                customLink2Value: 'CustomLink2Value',
                customLink3Value: 'CustomLink3Value'
            };

            Object.entries(fieldMap).forEach(([apiKey, dtoKey]) => {
                if (item.hasOwnProperty(dtoKey)) {
                    const el = document.querySelector(`[name="${dtoKey}"]`);
                    if (!el) return;
                    if (el.type === 'checkbox') el.checked = !!item[dtoKey];
                    else if (item[dtoKey] != null) el.value = item[dtoKey];
                }
            });

            bsModal?.show();
        } catch (err) {
            console.error('Error fetching item:', err);
            showFlash('Could not load item data. Please try again.', 'error');
        }
    }

    /* ------------------------------------------------------------------ */
    /* Save (add or update)                                                  */
    /* ------------------------------------------------------------------ */
    function bindSave() {
        document.getElementById('saveBtn')?.addEventListener('click', saveItem);
    }

    async function saveItem() {
        const saveBtn = document.getElementById('saveBtn');
        const isEdit = editingItemId !== null;
        const url = isEdit ? `/api/items/${editingItemId}` : '/api/items';
        const method = isEdit ? 'PUT' : 'POST';

        // Build DTO from form
        const dto = { inventoryId: INV_ID };

        // Collect form values with proper property names
        const fieldMap = {
            'CustomString1Value': 'CustomString1Value',
            'CustomString2Value': 'CustomString2Value',
            'CustomString3Value': 'CustomString3Value',
            'CustomText1Value': 'CustomText1Value',
            'CustomText2Value': 'CustomText2Value',
            'CustomText3Value': 'CustomText3Value',
            'CustomNumber1Value': 'CustomNumber1Value',
            'CustomNumber2Value': 'CustomNumber2Value',
            'CustomNumber3Value': 'CustomNumber3Value',
            'CustomBool1Value': 'CustomBool1Value',
            'CustomBool2Value': 'CustomBool2Value',
            'CustomBool3Value': 'CustomBool3Value',
            'CustomLink1Value': 'CustomLink1Value',
            'CustomLink2Value': 'CustomLink2Value',
            'CustomLink3Value': 'CustomLink3Value'
        };

        Object.keys(fieldMap).forEach(propName => {
            const el = document.querySelector(`[name="${propName}"]`);
            if (el) {
                if (el.type === 'checkbox') {
                    dto[propName] = el.checked;
                } else if (el.type === 'number') {
                    const val = el.value.trim();
                    dto[propName] = val ? parseFloat(val) : null;
                } else {
                    const val = el.value.trim();
                    dto[propName] = val || null;
                }
            }
        });

        // Add ID and Version for edit
        if (isEdit) {
            dto.id = editingItemId;
            dto.version = 1; // Could be retrieved from form if needed
        }

        // Spinner state
        saveBtn.disabled = true;
        saveBtn.classList.add('btn-saving');
        saveBtn.innerHTML = '<span class="spinner"></span>' + (isEdit ? 'Updating…' : 'Saving…');

        try {
            const resp = await fetch(url, {
                method,
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(dto)
            });

            if (resp.ok) {
                bsModal?.hide();
                // Show success flash BEFORE reload so it doesn't disappear
                sessionStorage.setItem('_flashMsg', isEdit ? 'Item updated successfully.' : 'Item added to inventory successfully.');
                location.reload();
            } else {
                const body = await resp.text().catch(() => '');
                showFlash(body || `Error ${resp.status}: Could not save item.`, 'error');
            }
        } catch (err) {
            console.error('Save error:', err);
            showFlash('Network error. Please try again.', 'error');
        } finally {
            saveBtn.disabled = false;
            saveBtn.classList.remove('btn-saving');
            saveBtn.textContent = isEdit ? 'Update Item' : 'Save Item';
        }
    }

    /* Restore flash message after reload */
    (function restoreFlash() {
        const msg = sessionStorage.getItem('_flashMsg');
        if (msg) {
            sessionStorage.removeItem('_flashMsg');
            // Wait for DOMContentLoaded handled above; show after paint
            document.addEventListener('DOMContentLoaded', () => showFlash(msg, 'success'));
        }
    })();

    /* ------------------------------------------------------------------ */
    /* Checkboxes — single selection only                                    */
    /* ------------------------------------------------------------------ */
    function bindCheckboxes() {
        if (!CAN_EDIT) return;

        document.querySelectorAll('.item-checkbox').forEach(cb => {
            cb.addEventListener('change', function () {
                if (this.checked) {
                    // Deselect all others (single-select)
                    document.querySelectorAll('.item-checkbox').forEach(other => {
                        if (other !== this) other.checked = false;
                    });
                }
                syncRowHighlight();
                updateToolbox();
            });
        });
    }

    function syncRowHighlight() {
        document.querySelectorAll('.item-row').forEach(row => {
            const cb = row.querySelector('.item-checkbox');
            row.classList.toggle('row-selected', cb?.checked ?? false);
        });
    }

    /* ------------------------------------------------------------------ */
    /* Toolbox                                                               */
    /* ------------------------------------------------------------------ */
    function bindToolbox() {
        if (!CAN_EDIT) return;

        document.getElementById('editBtn')?.addEventListener('click', () => {
            const cb = document.querySelector('.item-checkbox:checked');
            if (cb) openEditModal(Number(cb.value));
        });

        document.getElementById('deleteBtn')?.addEventListener('click', deleteSelected);

        document.getElementById('clearBtn')?.addEventListener('click', () => {
            document.querySelectorAll('.item-checkbox').forEach(cb => cb.checked = false);
            syncRowHighlight();
            updateToolbox();
        });
    }

    function updateToolbox() {
        const checked = document.querySelectorAll('.item-checkbox:checked').length;
        const toolbox = document.getElementById('toolbox');
        if (!toolbox) return;

        if (checked === 0) {
            toolbox.classList.add('d-none');
        } else {
            toolbox.classList.remove('d-none');
            const label = document.getElementById('selCount');
            if (label) label.textContent = `${checked} selected`;

            // Edit only enabled for exactly 1 selected
            const editBtn = document.getElementById('editBtn');
            if (editBtn) editBtn.disabled = checked !== 1;
        }
    }

    /* ------------------------------------------------------------------ */
    /* Delete                                                                */
    /* ------------------------------------------------------------------ */
    async function deleteSelected() {
        const cbs = [...document.querySelectorAll('.item-checkbox:checked')];
        if (!cbs.length) return;

        const count = cbs.length;
        const confirmed = confirm(`Delete ${count} item${count > 1 ? 's' : ''}? This cannot be undone.`);
        if (!confirmed) return;

        const deleteBtn = document.getElementById('deleteBtn');
        if (deleteBtn) { deleteBtn.disabled = true; deleteBtn.textContent = 'Deleting…'; }

        let errors = 0;
        for (const cb of cbs) {
            try {
                const r = await fetch(`/api/items/${cb.value}`, {
                    method: 'DELETE',
                    headers: { 'RequestVerificationToken': getAntiForgeryToken() }
                });
                if (!r.ok) errors++;
            } catch {
                errors++;
            }
        }

        const msg = errors
            ? `Deleted with ${errors} error(s). Page will refresh.`
            : `${count} item${count > 1 ? 's' : ''} deleted successfully.`;
        sessionStorage.setItem('_flashMsg', msg);
        location.reload();
    }

    /* ------------------------------------------------------------------ */
    /* Likes                                                                 */
    /* ------------------------------------------------------------------ */
    function bindLikeButtons() {
        document.querySelectorAll('.like-btn').forEach(btn => {
            btn.addEventListener('click', toggleLike);
        });
    }

    async function loadAllLikes() {
        const buttons = document.querySelectorAll('.like-btn');
        // Fetch all item like states in parallel
        await Promise.all([...buttons].map(btn => loadItemLikes(btn)));
    }

    async function loadItemLikes(btn) {
        const itemId = btn.dataset.itemId;
        try {
            const resp = await fetch(`/api/items/${itemId}/likes`);
            if (!resp.ok) return;
            const data = await resp.json();

            btn.querySelector('.like-count').textContent = data.count ?? 0;

            // data.userLiked or data.isLiked = whether the current authenticated user already liked
            const userLiked = IS_AUTH && (!!data.userLiked || !!data.isLiked);
            btn.dataset.liked = userLiked ? 'true' : 'false';
            btn.querySelector('.heart-icon').textContent = userLiked ? '♥' : '♡';
            btn.classList.toggle('liked', userLiked);
        } catch (err) {
            console.error('Error loading likes:', err);
            btn.querySelector('.like-count').textContent = '—';
        }
    }

    async function toggleLike(e) {
        const btn = e.currentTarget;

        if (!IS_AUTH) {
            showFlash('Please sign in to like items.', 'error');
            return;
        }

        const itemId = btn.dataset.itemId;

        // Optimistic UI
        const wasLiked = btn.dataset.liked === 'true';
        const countEl = btn.querySelector('.like-count');
        const heartEl = btn.querySelector('.heart-icon');
        const prevCount = parseInt(countEl.textContent, 10) || 0;

        btn.dataset.liked = wasLiked ? 'false' : 'true';
        countEl.textContent = wasLiked ? Math.max(0, prevCount - 1) : prevCount + 1;
        heartEl.textContent = wasLiked ? '♡' : '♥';
        btn.classList.toggle('liked', !wasLiked);
        btn.disabled = true;

        try {
            const resp = await fetch(`/api/items/${itemId}/like`, {
                method: 'POST',
                headers: { 'RequestVerificationToken': getAntiForgeryToken() }
            });
            if (resp.ok) {
                const data = await resp.json();
                countEl.textContent = data.likeCount ?? countEl.textContent;
                const serverLiked = !!data.liked;
                btn.dataset.liked = serverLiked ? 'true' : 'false';
                heartEl.textContent = serverLiked ? '♥' : '♡';
                btn.classList.toggle('liked', serverLiked);
            } else {
                // Roll back optimistic update
                rollbackLike(btn, wasLiked, prevCount);
            }
        } catch {
            rollbackLike(btn, wasLiked, prevCount);
        } finally {
            btn.disabled = false;
        }
    }

    function rollbackLike(btn, wasLiked, prevCount) {
        btn.dataset.liked = wasLiked ? 'true' : 'false';
        btn.querySelector('.like-count').textContent = prevCount;
        btn.querySelector('.heart-icon').textContent = wasLiked ? '♥' : '♡';
        btn.classList.toggle('liked', wasLiked);
    }

    /* ------------------------------------------------------------------ */
    /* Flash messages                                                        */
    /* ------------------------------------------------------------------ */
    function showFlash(msg, type = 'success') {
        const container = document.getElementById('flashContainer');
        if (!container) return;

        const el = document.createElement('div');
        el.className = `flash-alert ${type}`;
        el.setAttribute('role', 'alert');
        el.innerHTML = `
                <span>${escapeHtml(msg)}</span>
                <button class="flash-close" aria-label="Dismiss">×</button>
            `;
        el.querySelector('.flash-close').addEventListener('click', () => el.remove());
        container.appendChild(el);

        if (type === 'success') {
            setTimeout(() => el.classList.add('fade') || el.remove(), 5000);
        }
    }

    /* ------------------------------------------------------------------ */
    /* Helpers                                                               */
    /* ------------------------------------------------------------------ */
    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
    }

    function escapeHtml(str) {
        return String(str).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    }

    /* Restore any flash message that was stored before page reload */
    const _pendingFlash = sessionStorage.getItem('_flashMsg');
    if (_pendingFlash) {
        sessionStorage.removeItem('_flashMsg');
        document.addEventListener('DOMContentLoaded', () => showFlash(_pendingFlash, 'success'));
    }
})();