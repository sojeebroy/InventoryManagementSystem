

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
                    this.lastUpdate = new Date();

                    const event = new CustomEvent('discussionsUpdated', {
                        detail: { discussions: newDiscussions }
                    });
                    window.dispatchEvent(event);
                }
            }
        } catch (error) {
            console.warn('Error polling discussions:', error);
        }

        const jitter = Math.random() * 1000;
        this.pollTimeoutId = setTimeout(() => this.poll(), this.pollInterval + jitter);
    }
}

window.addEventListener('discussionsUpdated', (event) => {
    const discussions = event.detail.discussions;
    console.log(`${discussions.length} new discussion(s) received`);

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

document.addEventListener('DOMContentLoaded', () => {
    if (window.inventoryId) {
        window.discussionRealtime = new DiscussionRealtime(window.inventoryId, 3000);

        document.getElementById('chat-tab')?.addEventListener('click', () => {
            window.discussionRealtime.start();
        });

        document.addEventListener('hidden.bs.tab', (e) => {
            if (e.target.id === 'chat-tab') {
                window.discussionRealtime.stop();
            }
        });
    }
});

