const connectionStatus = document.getElementById('connection');
const lastCheck = document.getElementById('last-check');
const notifications = document.getElementById('notifications');
const history = document.getElementById('history');
const historySummary = document.getElementById('history-summary');
const buttons = [...document.querySelectorAll('button')];
let running = false;

function setStatus(text, passed) {
	lastCheck.textContent = text;
	lastCheck.className = passed ? 'good' : 'bad';
}

function prepend(list, text, passed) {
	const item = document.createElement('li');
	item.textContent = text;
	item.className = passed === false ? 'bad' : 'good';
	list.prepend(item);
}

function appendMuted(list, text) {
	const item = document.createElement('li');
	item.textContent = text;
	item.className = 'muted';
	list.append(item);
}

function renderHistory(entries) {
	history.replaceChildren();

	if (entries.length === 0) {
		historySummary.textContent = 'No stored notification entries on the server.';
		appendMuted(history, 'No stored notification entries yet.');
		return;
	}

	const last = new Date(entries[0].completedAt).toLocaleTimeString();
	historySummary.textContent = `${entries.length} stored notification entries. Last stored at ${last}.`;

	for (const entry of entries)
		appendMuted(history, `${entry.status}: ${entry.subject} -> ${entry.recipient} (${new Date(entry.completedAt).toLocaleTimeString()})`);
}

async function refreshHistory() {
	try {
		// Stored history is server state. Refreshing it should always re-render the visible history list.
		const response = await fetch('/dashboard/history', { cache: 'no-store' });
		if (!response.ok)
			throw new Error(`HTTP ${response.status}`);

		const entries = await response.json();
		renderHistory(entries);
		setStatus(`PASS: refreshed ${entries.length} stored notification entries at ${new Date().toLocaleTimeString()}.`, true);
	} catch (error) {
		setStatus(`FAIL: stored history refresh failed: ${error}`, false);
	}
}

function clearBrowserView() {
	// This is intentionally browser-only. It does not call the server and does not clear MemoryNotificationStore.
	notifications.replaceChildren();
	history.replaceChildren();
	historySummary.textContent = 'Browser view cleared. Server stored history was not cleared.';
	lastCheck.textContent = 'Browser view cleared. Click Refresh stored history to read server entries again.';
	lastCheck.className = 'muted';
}

async function runExclusive(action) {
	if (running)
		return;

	running = true;
	buttons.forEach(button => button.disabled = true);

	try {
		await action();
	} finally {
		running = false;
		buttons.forEach(button => button.disabled = false);
	}
}

const connection = new signalR.HubConnectionBuilder()
	.withUrl('/notifications?user=demo')
	.build();

connection.on('ReceiveNotification', notification => {
	prepend(notifications, `${notification.subject}: ${notification.body}`, true);
	setStatus('PASS: browser received SignalR notification.', true);
});

document.getElementById('send').addEventListener('click', () => runExclusive(async () => {
	const response = await fetch('/dashboard/notify', { method: 'POST', cache: 'no-store' });
	if (!response.ok) {
		setStatus('FAIL: server rejected notification send.', false);
		return;
	}

	await response.json();
	setStatus('PASS: server accepted notification send.', true);
	await refreshHistory();
}));

document.getElementById('check').addEventListener('click', () => runExclusive(async () => {
	const response = await fetch('/dashboard/self-check', { method: 'POST', cache: 'no-store' });
	const result = await response.json();
	setStatus(`${result.passed ? 'PASS' : 'FAIL'}: ${result.message}`, result.passed);
	await refreshHistory();
}));

document.getElementById('refresh').addEventListener('click', () => runExclusive(refreshHistory));
document.getElementById('clear').addEventListener('click', () => runExclusive(async () => clearBrowserView()));

connection.start()
	.then(async () => {
		connectionStatus.textContent = 'Connected to SignalR as user demo.';
		connectionStatus.className = 'good';
		await refreshHistory();
	})
	.catch(error => {
		connectionStatus.textContent = `SignalR connection failed: ${error}`;
		connectionStatus.className = 'bad';
	});
