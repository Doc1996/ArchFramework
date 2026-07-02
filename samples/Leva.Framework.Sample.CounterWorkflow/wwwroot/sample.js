const runButton = document.getElementById('run');
const summary = document.getElementById('summary');
const checks = document.getElementById('checks');
const runtime = document.getElementById('runtime');
const executions = document.getElementById('executions');
let running = false;

function renderList(list, items, createText, className) {
	list.replaceChildren();

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
		const response = await fetch('/workflow/run', { method: 'POST' });
		const result = await response.json();

		document.getElementById('state').textContent = result.state;
		document.getElementById('count').textContent = result.count;
		document.getElementById('runtime-count').textContent = result.runtimeEntryCount;
		document.getElementById('execution-count').textContent = result.executionEntryCount;
		document.getElementById('report').textContent = result.report;

		renderList(checks, result.checks, check => `${check.passed ? 'PASS' : 'FAIL'}: ${check.name}`, check => check.passed ? 'good' : 'bad');
		renderList(runtime, result.runtimeLog, entry => `${entry.category}: ${entry.message} (${entry.source})`, 'muted');
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
