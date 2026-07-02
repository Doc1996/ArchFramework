const runButton = document.getElementById('run');
const summary = document.getElementById('summary');
const scenarios = document.getElementById('scenarios');
const checks = document.getElementById('checks');
let running = false;

function renderChecks(items) {
	checks.replaceChildren();

	for (const check of items) {
		const item = document.createElement('li');
		item.textContent = `${check.passed ? 'PASS' : 'FAIL'}: ${check.name}`;
		item.className = check.passed ? 'good' : 'bad';
		checks.append(item);
	}
}

function renderScenarios(items) {
	scenarios.replaceChildren();

	for (const scenario of items) {
		const card = document.createElement('div');
		card.className = 'card';
		card.innerHTML = `
			<h3 class="${scenario.passed ? 'good' : 'bad'}">${scenario.provider}: ${scenario.passed ? 'PASS' : 'FAIL'}</h3>
			<p><strong>Ticket:</strong> ${scenario.ticketTitle || '-'}</p>
			<p><strong>Assigned to:</strong> ${scenario.assignedTo || '-'}</p>
			<p><strong>Status:</strong> ${scenario.status || '-'}</p>
			<p><strong>Activity entries:</strong> ${scenario.activityEntries}</p>
			<p><strong>Notification delta:</strong> sent=${scenario.notificationsSent}, stored=${scenario.notificationEntriesStored}</p>
			<p class="muted">${scenario.message}</p>`;
		scenarios.append(card);
	}
}

async function runTicketDesk() {
	if (running)
		return;

	running = true;
	runButton.disabled = true;
	summary.textContent = 'Running...';
	summary.className = 'muted';

	try {
		const response = await fetch('/ticketdesk/run', { method: 'POST' });
		const result = await response.json();

		document.getElementById('sent').textContent = result.notificationsSent;
		document.getElementById('stored').textContent = result.notificationEntriesStored;
		renderScenarios(result.scenarios);
		renderChecks(result.checks);

		summary.textContent = result.passed ? 'Sample result: PASS' : 'Sample result: FAIL';
		summary.className = result.passed ? 'good' : 'bad';
	} catch (error) {
		summary.textContent = `Sample request failed: ${error}`;
		summary.className = 'bad';
	} finally {
		running = false;
		runButton.disabled = false;
	}
}

runButton.addEventListener('click', runTicketDesk);
