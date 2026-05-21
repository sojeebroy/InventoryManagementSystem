/**
 * Real-time Discussion Updates
 * Polls for new discussion posts every 3-5 seconds
 */

class DiscussionRealtime {
    constructor(inventoryId, pollInterval = 3000) {
        this.inventoryId = inventoryId;
        this.pollInterval = pollInterval;
        this.lastUpdate = new Date();
        this.isPolling = false;
        this.pollTimeoutId = null;
    }

    start() {
        if (this.isPolling) return;

        this.isPolling = true;
        this.lastUpdate = new Date();
        this.poll();
    }

    stop() {
        this.isPolling = false;
        if (this.pollTimeoutId) {
            clearTimeout(this.pollTimeoutId);
        }
    }

    async poll() {
        if (!this.isPolling) return;

        try {
            const response = await fetch(`/Inventories/GetDiscussionsSince?inventoryId=${this.inventoryId}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(this.lastUpdate)
            });

            if (response.ok) {
                const newDiscussions = await response.json();
                if (newDiscussions && newDiscussions.length > 0) {
                    // Update last poll time
                    this.lastUpdate = new Date();

                    // Trigger custom event for new discussions
                    const event = new CustomEvent('discussionsUpdated', {
                        detail: { discussions: newDiscussions }
                    });
                    window.dispatchEvent(event);
                }
            }
        } catch (error) {
            console.warn('Error polling discussions:', error);
        }

        // Schedule next poll with slight randomization to avoid thundering herd
        const jitter = Math.random() * 1000; // 0-1 second random jitter
        this.pollTimeoutId = setTimeout(() => this.poll(), this.pollInterval + jitter);
    }
}

// Auto-reload content on updates
window.addEventListener('discussionsUpdated', (event) => {
    const discussions = event.detail.discussions;
    console.log(`${discussions.length} new discussion(s) received`);

    // Reload discussion container if visible
    const chatContainer = document.getElementById('chatContainer');
    if (chatContainer && !chatContainer.classList.contains('d-none')) {
        reloadDiscussionTab();
    }
});

async function reloadDiscussionTab() {
    const inventoryId = window.inventoryId;
    if (!inventoryId) return;

    try {
        const response = await fetch(`/Inventories/DiscussionPartial/${inventoryId}?page=1`);
        if (response.ok) {
            const html = await response.text();
            const chatContainer = document.getElementById('chatContainer');
            if (chatContainer) {
                chatContainer.innerHTML = html;
            }
        }
    } catch (error) {
        console.error('Error reloading discussion:', error);
    }
}

// Initialize when Settings page loads
document.addEventListener('DOMContentLoaded', () => {
    if (window.inventoryId) {
        window.discussionRealtime = new DiscussionRealtime(window.inventoryId, 3000);

        // Start polling when user clicks on discussion tab
        document.getElementById('chat-tab')?.addEventListener('click', () => {
            window.discussionRealtime.start();
        });

        // Stop polling when user leaves discussion tab
        document.addEventListener('hidden.bs.tab', (e) => {
            if (e.target.id === 'chat-tab') {
                window.discussionRealtime.stop();
            }
        });
    }
});
