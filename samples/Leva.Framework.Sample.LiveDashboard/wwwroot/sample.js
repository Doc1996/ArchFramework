const connectionStatus = document.getElementById('connection');
const lastCheck = document.getElementById('last-check');
const notifications = document.getElementById('notifications');
const history = document.getElementById('history');
const sendButton = document.getElementById('send');
const checkButton = document.getElementById('check');
const refreshButton = document.getElementById('refresh');
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

function renderHistory(entries) {
	history.replaceChildren();

	if (entries.length === 0) {
		const item = document.createElement('li');
		item.textContent = 'No stored notification entries yet.';
		item.className = 'muted';
		history.append(item);
		return;
	}

	for (const entry of entries) {
		const item = document.createElement('li');
		item.textContent = `${entry.status}: ${entry.subject} -> ${entry.recipient} (${new Date(entry.completedAt).toLocaleTimeString()})`;
		item.className = 'muted';
		history.append(item);
	}
}

async function refreshHistory() {
	try {
		// Stored history is read from the sample's MemoryNotificationStore through a DTO endpoint.
		const response = await fetch('/dashboard/history', { cache: 'no-store' });
		if (!response.ok)
			throw new Error(`HTTP ${response.status}`);

		const entries = await response.json();
		renderHistory(entries);
		setStatus(`PASS: refreshed ${entries.length} stored notification entries.`, true);
	} catch (error) {
		setStatus(`FAIL: stored history refresh failed: ${error}`, false);
	}
}

async function runExclusive(action) {
	if (running)
		return;

	running = true;
	sendButton.disabled = true;
	checkButton.disabled = true;
	refreshButton.disabled = true;

	try {
		await action();
	} finally {
		running = false;
		sendButton.disabled = false;
		checkButton.disabled = false;
		refreshButton.disabled = false;
	}
}

const connection = new signalR.HubConnectionBuilder()
	.withUrl('/notifications?user=demo')
	.build();

connection.on('ReceiveNotification', notification => {
	// SignalR proves live delivery. Stored history is refreshed explicitly after endpoint calls.
	prepend(notifications, `${notification.subject}: ${notification.body}`, true);
	setStatus('PASS: browser received SignalR notification.', true);
});

sendButton.addEventListener('click', () => runExclusive(async () => {
	const response = await fetch('/dashboard/notify', { method: 'POST', cache: 'no-store' });
	if (!response.ok) {
		setStatus('FAIL: server rejected notification send.', false);
		return;
	}

	// Wait until NotificationService has returned before reading stored history.
	await response.json();
	setStatus('PASS: server accepted notification send.', true);
	await refreshHistory();
}));

checkButton.addEventListener('click', () => runExclusive(async () => {
	const response = await fetch('/dashboard/self-check', { method: 'POST', cache: 'no-store' });
	const result = await response.json();
	setStatus(`${result.passed ? 'PASS' : 'FAIL'}: ${result.message}`, result.passed);
	await refreshHistory();
}));

refreshButton.addEventListener('click', () => runExclusive(refreshHistory));

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
