
(function () {
    function initInventoryFields(rootElement, inventoryId) {
        const root = rootElement instanceof Element ? rootElement : document;
        inventoryId = typeof inventoryId === 'number' ? inventoryId : parseInt(window.inventoryId, 10);

        const fieldsContainer = root.querySelector('#fieldsContainer');
        const addFieldBtn = root.querySelector('#addFieldBtn');
        const fieldModalEl = root.querySelector('#fieldModal');
        const fieldForm = root.querySelector('#fieldForm');
        const fieldModalTitle = root.querySelector('#fieldModalTitle');
        const deleteFieldModalBtn = root.querySelector('#deleteFieldModalBtn');
        const saveFieldBtn = root.querySelector('#saveFieldBtn');
        const fieldTypeSelect = root.querySelector('#fieldTypeSelect');
        const savingIndicator = root.querySelector('#savingIndicator');
        const savedIndicator = root.querySelector('#savedIndicator');
        const fieldLimitAlert = root.querySelector('#fieldLimitAlert');
        const fieldErrorAlert = root.querySelector('#fieldErrorAlert');
        const fieldErrorMessage = root.querySelector('#fieldErrorMessage');

        if (!fieldsContainer || !fieldModalEl) return;

        const fieldModal = new bootstrap.Modal(fieldModalEl);

        function showSavingStatus() {
            if (savingIndicator) savingIndicator.classList.remove('d-none');
            if (savedIndicator) savedIndicator.classList.add('d-none');
        }
        function showSavedStatus() {
            if (savingIndicator) savingIndicator.classList.add('d-none');
            if (savedIndicator) {
                savedIndicator.classList.remove('d-none');
                setTimeout(() => savedIndicator.classList.add('d-none'), 3000);
            }
        }
        function showFieldError(message) {
            if (!fieldErrorAlert || !fieldErrorMessage) return;
            fieldErrorMessage.textContent = message;
            fieldErrorAlert.style.display = 'block';
            setTimeout(() => { fieldErrorAlert.style.display = 'none'; }, 5000);
        }
        function escapeHtml(text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }
        function getRequestVerificationToken() {
            return document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        }

        function updateFieldLimitStatus() {
            const fieldTypeMaxLimits = 3;
            const counts = { SingleLineText: 0, MultiLineText: 0, Numeric: 0, Boolean: 0, Link: 0 };
            fieldsContainer.querySelectorAll('.field-item').forEach(el => {
                const t = el.querySelector('small')?.textContent || '';
                if (t.includes('SingleLineText')) counts.SingleLineText++;
                else if (t.includes('MultiLineText')) counts.MultiLineText++;
                else if (t.includes('Numeric')) counts.Numeric++;
                else if (t.includes('Boolean')) counts.Boolean++;
                else if (t.includes('Link')) counts.Link++;
            });

            if (fieldLimitAlert) {
                const hasLimit = Object.values(counts).some(c => c >= fieldTypeMaxLimits);
                fieldLimitAlert.style.display = hasLimit ? 'block' : 'none';
            }

            if (fieldTypeSelect) {
                const selected = fieldTypeSelect.value;
                Array.from(fieldTypeSelect.options).forEach(option => {
                    if (!option.value) return;
                    const ct = counts[option.value] || 0;
                    if (ct >= fieldTypeMaxLimits && option.value !== selected) {
                        option.disabled = true;
                        const base = option.textContent.split(' (')[0];
                        option.textContent = `${base} (${ct}/${fieldTypeMaxLimits})`;
                    } else {
                        option.disabled = false;
                    }
                });
            }
        }

        function addFieldToUI(fieldId, title, fieldType, description, isVisible) {
            const empty = fieldsContainer.querySelector('.alert-secondary');
            if (empty) empty.remove();

            const el = document.createElement('div');
            el.className = 'card mb-2 field-item';
            el.dataset.fieldId = fieldId;
            el.dataset.fieldTitle = title;
            el.dataset.fieldType = fieldType;
            el.dataset.fieldDescription = description;
            el.dataset.fieldVisible = isVisible;
            el.setAttribute('draggable', 'true');
            el.style.backgroundColor = '#f8f9fa';
            el.style.cursor = 'move';

            el.innerHTML = `
                <div class="card-body p-3">
                    <div class="row align-items-center">
                        <div class="col">
                            <div class="d-flex align-items-center gap-2">
                                <i class="bi bi-grip-vertical text-muted"></i>
                                <div>
                                    <h6 class="mb-1">${escapeHtml(title)}</h6>
                                    <small class="text-muted">${fieldType}${description ? ' • ' + escapeHtml(description) : ''}</small>
                                </div>
                            </div>
                        </div>
                        <div class="col-auto">
                            <span class="badge ${isVisible ? 'bg-info' : 'bg-secondary'}">${isVisible ? 'Visible' : 'Hidden'}</span>
                            <button type="button" class="btn btn-sm btn-outline-warning editFieldBtn ms-2" data-field-id="${fieldId}">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-outline-danger deleteFieldBtn" data-field-id="${fieldId}">
                                <i class="bi bi-trash"></i>
                            </button>
                        </div>
                    </div>
                </div>
            `;

            fieldsContainer.appendChild(el);
            attachDragListeners(el);
        }

        function updateFieldInUI(fieldId, title, fieldType, description, isVisible) {
            const el = fieldsContainer.querySelector(`[data-field-id="${fieldId}"]`);
            if (!el) return;
            const titleEl = el.querySelector('h6');
            const smallEl = el.querySelector('small');
            const badge = el.querySelector('.badge');
            if (titleEl) titleEl.textContent = title;
            if (smallEl) smallEl.textContent = `${fieldType}${description ? ' • ' + description : ''}`;
            if (badge) {
                badge.textContent = isVisible ? 'Visible' : 'Hidden';
                badge.className = isVisible ? 'badge bg-info' : 'badge bg-secondary';
            }
        }

        function removeFieldFromUI(fieldId) {
            const el = fieldsContainer.querySelector(`[data-field-id="${fieldId}"]`);
            if (!el) return;
            el.remove();
            if (fieldsContainer.querySelectorAll('.field-item').length === 0) {
                const emptyAlert = document.createElement('div');
                emptyAlert.className = 'alert alert-secondary text-center py-4';
                emptyAlert.innerHTML = '<p class="mb-0">No custom fields added yet. Click \"Add Custom Field\" to get started.</p>';
                fieldsContainer.appendChild(emptyAlert);
            }
        }

        let draggedElement = null;
        function attachDragListeners(item) {
            item.addEventListener('dragstart', (e) => {
                draggedElement = item;
                item.style.opacity = '0.5';
                item.style.backgroundColor = '#e9ecef';
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData('text/html', item.innerHTML);
            });
            item.addEventListener('dragend', () => {
                item.style.opacity = '1';
                item.style.backgroundColor = '#f8f9fa';
                draggedElement = null;
            });
        }

        fieldsContainer.querySelectorAll('.field-item').forEach(el => {
            if (!el.dataset.fieldTitle) {
                const title = el.querySelector('h6')?.textContent || '';
                const typeText = el.querySelector('small')?.textContent || '';
                const descriptionPart = typeText.split('•')[1]?.trim() || '';
                const isVisible = el.querySelector('.badge').classList.contains('bg-info');
                el.dataset.fieldTitle = title;
                el.dataset.fieldType = typeText.split('•')[0]?.trim() || typeText;
                el.dataset.fieldDescription = descriptionPart;
                el.dataset.fieldVisible = isVisible;
            }
            attachDragListeners(el);
        });

        fieldsContainer.addEventListener('dragover', e => {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
            const after = getDragAfterElement(fieldsContainer, e.clientY);
            if (!after) fieldsContainer.appendChild(draggedElement);
            else fieldsContainer.insertBefore(draggedElement, after);
        });

        fieldsContainer.addEventListener('drop', () => {
            const REORDER_DEBOUNCE_DELAY = 1000;
            if (fieldsContainer._reorderTimeout) clearTimeout(fieldsContainer._reorderTimeout);
            fieldsContainer._reorderTimeout = setTimeout(async () => {
                const newOrder = Array.from(fieldsContainer.querySelectorAll('.field-item')).map((el, idx) => {
                    const fieldId = parseInt(el.dataset.fieldId, 10);
                    return { id: fieldId, order: idx };
                });
                try {
                    showSavingStatus();
                    const token = getRequestVerificationToken();
                    const res = await fetch(`/Inventories/ReorderCustomFields?inventoryId=${inventoryId}`, {
                        method: 'POST',
                        credentials: 'same-origin',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': token,
                            'X-CSRF-TOKEN': token,
                            'X-XSRF-TOKEN': token,
                            '__RequestVerificationToken': token,
                            'X-Requested-With': 'XMLHttpRequest'
                        },
                        body: JSON.stringify(newOrder)
                    });
                    const json = await res.json();
                    if (json.success) showSavedStatus();
                    else showFieldError('Error reordering fields');
                } catch (err) {
                    console.error(err);
                    showFieldError('Error reordering fields. Please try again.');
                }
            }, REORDER_DEBOUNCE_DELAY);
        });

        function getDragAfterElement(container, y) {
            const draggable = [...container.querySelectorAll('.field-item')].filter(el => el !== draggedElement);
            let closest = { offset: Number.NEGATIVE_INFINITY, element: null };
            for (const child of draggable) {
                const box = child.getBoundingClientRect();
                const offset = y - box.top - box.height / 2;
                if (offset < 0 && offset > closest.offset) {
                    closest = { offset: offset, element: child };
                }
            }
            return closest.element;
        }

        if (addFieldBtn) {
            addFieldBtn.addEventListener('click', () => {
                if (fieldModalTitle) fieldModalTitle.textContent = 'Add Custom Field';
                if (fieldForm) fieldForm.reset();
                const idInput = root.querySelector('#fieldIdInput');
                if (idInput) idInput.value = '';
                if (fieldTypeSelect) {
                    fieldTypeSelect.disabled = false;
                    fieldTypeSelect.value = '';
                }
                if (deleteFieldModalBtn) deleteFieldModalBtn.style.display = 'none';
                updateFieldLimitStatus();
                fieldModal.show();
            });
        }

        fieldsContainer.addEventListener('click', (e) => {
            const editBtn = e.target.closest('.editFieldBtn');
            if (editBtn) {
                const fieldId = editBtn.dataset.fieldId;
                const item = fieldsContainer.querySelector(`[data-field-id="${fieldId}"]`);
                if (!item) return;
                const title = item.querySelector('h6')?.textContent || '';
                const typeText = item.querySelector('small')?.textContent || '';
                const description = typeText.split('•')[1]?.trim() || '';
                let type = '';
                if (typeText.includes('SingleLineText')) type = 'SingleLineText';
                else if (typeText.includes('MultiLineText')) type = 'MultiLineText';
                else if (typeText.includes('Numeric')) type = 'Numeric';
                else if (typeText.includes('Boolean')) type = 'Boolean';
                else if (typeText.includes('Link')) type = 'Link';
                const isVisible = item.querySelector('.badge').classList.contains('bg-info');

                if (fieldModalTitle) fieldModalTitle.textContent = 'Edit Custom Field';
                const idInput = root.querySelector('#fieldIdInput'); if (idInput) idInput.value = fieldId;
                const nameInput = root.querySelector('#fieldNameInput'); if (nameInput) nameInput.value = title;
                if (fieldTypeSelect) { fieldTypeSelect.value = type; fieldTypeSelect.any = true; }
                const descInput = root.querySelector('#fieldDescriptionInput'); if (descInput) descInput.value = description;
                const visChk = root.querySelector('#fieldVisibleCheck'); if (visChk) visChk.checked = isVisible;
                if (deleteFieldModalBtn) deleteFieldModalBtn.style.display = 'block';
                fieldModal.show();
            }
        });

        fieldsContainer.addEventListener('click', async (e) => {
            const delBtn = e.target.closest('.deleteFieldBtn');
            if (delBtn) {
                const fieldId = delBtn.dataset.fieldId;
                if (!fieldId) return;
                if (!confirm('Are you sure you want to delete this field? All data in this field will be lost.')) return;

                try {
                    showSavingStatus();
                    removeFieldFromUI(fieldId);

                    const token = getRequestVerificationToken();
                    const res = await fetch(`/Inventories/DeleteCustomField?id=${fieldId}`, {
                        method: 'POST',
                        credentials: 'same-origin',
                        headers: {
                            'RequestVerificationToken': token,
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    });

                    if (!res.ok) {
                        throw new Error(`HTTP error! status: ${res.status}`);
                    }

                    const json = await res.json();
                    if (json.success) {
                        showSavedStatus();
                        updateFieldLimitStatus();
                        setTimeout(() => {
                            location.reload();
                        }, 500);
                    } else {
                        location.reload();
                        showFieldError('Error deleting field');
                    }
                } catch (err) {
                    console.error('Delete error:', err);
                    location.reload();
                    showFieldError('Error deleting field. Please try again.');
                }
            }
        });

        if (deleteFieldModalBtn) {
            deleteFieldModalBtn.addEventListener('click', async () => {
                const idInput = root.querySelector('#fieldIdInput');
                if (!idInput) return;

                const fieldId = idInput.value;
                if (!fieldId) return;
                if (!confirm('Are you sure you want to delete this field? All data in this field will be lost.')) return;

                try {
                    showSavingStatus();
                    removeFieldFromUI(fieldId);

                    const token = getRequestVerificationToken();
                    const res = await fetch(`/Inventories/DeleteCustomField?id=${fieldId}`, {
                        method: 'POST',
                        credentials: 'same-origin',
                        headers: {
                            'RequestVerificationToken': token,
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    });

                    if (!res.ok) {
                        throw new Error(`HTTP error! status: ${res.status}`);
                    }

                    const json = await res.json();
                    if (json.success) {
                        fieldModal.hide();
                        showSavedStatus();
                        updateFieldLimitStatus();
                        setTimeout(() => {
                            location.reload();
                        }, 500);
                    } else {
                        location.reload();
                        showFieldError('Error deleting field');
                    }
                } catch (err) {
                    console.error('Delete error:', err);
                    location.reload();
                    showFieldError('Error deleting field. Please try again.');
                }
            });
        }

        if (saveFieldBtn) {
            saveFieldBtn.addEventListener('click', async () => {
                const idInput = root.querySelector('#fieldIdInput');
                const nameInput = root.querySelector('#fieldNameInput');
                const typeSelect = fieldTypeSelect;
                const descInput = root.querySelector('#fieldDescriptionInput');
                const visChk = root.querySelector('#fieldVisibleCheck');

                const fieldId = idInput?.value;
                const fieldName = nameInput?.value?.trim() || '';
                const fieldType = typeSelect?.value || '';
                const description = descInput?.value || '';
                const isVisible = !!visChk?.checked;

                if (!fieldName || !fieldType) {
                    showFieldError('Please fill in all required fields');
                    return;
                }
                if (fieldName.length > 100) { showFieldError('Field name must be 100 characters or less'); return; }
                if (description.length > 500) { showFieldError('Description must be 500 characters or less'); return; }

                try {
                    showSavingStatus();
                    const url = fieldId ? `/Inventories/UpdateCustomField?id=${fieldId}` : `/Inventories/CreateCustomField?inventoryId=${inventoryId}`;
                    const token = getRequestVerificationToken();
                    const res = await fetch(url, {
                        method: 'POST',
                        credentials: 'same-origin',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': token,
                            'X-CSRF-TOKEN': token,
                            'X-XSRF-TOKEN': token,
                            '__RequestVerificationToken': token,
                            'X-Requested-With': 'XMLHttpRequest'
                        },
                        body: JSON.stringify({
                            title: fieldName,
                            description: description || null,
                            fieldType: fieldType,
                            isVisibleInTable: isVisible
                        })
                    });

                    if (!res.ok) {
                        throw new Error(`HTTP error! status: ${res.status}`);
                    }

                    const json = await res.json();
                    if (json.success) {
                        if (fieldId) {
                            updateFieldInUI(fieldId, fieldName, fieldType, description, isVisible);
                            fieldModal.hide();
                            showSavedStatus();
                            updateFieldLimitStatus();
                        } else {
                            const newId = json.field?.id || Date.now();
                            const newType = json.field?.fieldType || fieldType;
                            addFieldToUI(newId, fieldName, newType, description, isVisible);
                            fieldModal.hide();
                            showSavedStatus();
                            updateFieldLimitStatus();
                        }
                    } else {
                        showFieldError(json.error || 'Unknown error occurred');
                    }
                } catch (err) {
                    console.error('Field save error:', err);
                    showFieldError('Error saving field. Please try again.');
                }
            });
        }

        updateFieldLimitStatus();
    }

    window.initInventoryFields = initInventoryFields;
})();
