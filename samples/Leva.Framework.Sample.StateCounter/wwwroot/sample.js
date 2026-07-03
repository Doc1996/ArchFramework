const runButton = document.getElementById('run');
const summary = document.getElementById('summary');
const checks = document.getElementById('checks');
const runtime = document.getElementById('runtime');
const executions = document.getElementById('executions');
const alarms = document.getElementById('alarms');
const statuses = document.getElementById('statuses');
let running = false;

function friendlyStatus(status) {
	if (status.source === 'StateCounter' && status.name === 'TargetCount')
		return `Target count: ${status.value}`;

	return `${status.source}.${status.name}: ${status.value}`;
}

function renderList(list, items, createText, className) {
	list.replaceChildren();

	if (items.length === 0) {
		const element = document.createElement('li');
		element.textContent = 'No entries.';
		element.className = 'muted';
		list.append(element);
		return;
	}

	for (const item of items) {
		const element = document.createElement('li');
		element.textContent = createText(item);
		element.className = typeof className === 'function' ? className(item) : className;
		list.append(element);
	}
}

async function runWorkflow() {
	if (running)
		return;

	running = true;
	runButton.disabled = true;
	summary.textContent = 'Running...';
	summary.className = 'muted';

	try {
		const response = await fetch('/workflow/run', { method: 'POST', cache: 'no-store' });
		const result = await response.json();

		document.getElementById('state').textContent = result.state;
		document.getElementById('count').textContent = result.count;
		document.getElementById('runtime-count').textContent = result.runtimeEntryCount;
		document.getElementById('execution-count').textContent = result.executionEntryCount;
		document.getElementById('alarm-count').textContent = result.alarmCount;
		document.getElementById('status-count').textContent = result.statusCount;
		document.getElementById('report').textContent = result.report;

		renderList(checks, result.checks, check => `${check.passed ? 'PASS' : 'FAIL'}: ${check.name}`, check => check.passed ? 'good' : 'bad');
		renderList(alarms, result.alarms, alarm => `${alarm.level}: ${alarm.message}`, 'muted');
		renderList(statuses, result.statuses, status => friendlyStatus(status), 'muted');
		renderList(runtime, result.runtimeLog, entry => `${entry.category}: ${entry.message}`, 'muted');
		renderList(executions, result.executions, entry => `${entry.name}: ${entry.status} ${entry.progressMessage ?? ''}`, 'muted');

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

runButton.addEventListener('click', runWorkflow);
