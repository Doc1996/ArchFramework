const checks = document.getElementById('checks');
const output = document.getElementById('output');
const buttons = [...document.querySelectorAll('button')];
let running = false;

function writeResponse(value) {
	output.textContent = typeof value === 'string' ? value : JSON.stringify(value, null, 2);
}

function addCheck(name, passed) {
	const item = document.createElement('li');
	item.textContent = `${passed ? 'PASS' : 'FAIL'}: ${name}`;
	item.className = passed ? 'good' : 'bad';
	checks.append(item);
	return passed;
}

async function readBody(response) {
	const text = await response.text();
	try {
		return text.length === 0 ? null : JSON.parse(text);
	} catch {
		return text;
	}
}

async function postLogout() {
	const response = await fetch('/sample/logout', { method: 'POST', credentials: 'include', cache: 'no-store' });
	return { response, body: await readBody(response) };
}

async function getPrincipal() {
	const response = await fetch('/identity/principal', { credentials: 'include', cache: 'no-store' });
	return { response, body: await readBody(response) };
}

async function getAdminArea() {
	const response = await fetch('/account/admin-area', { credentials: 'include', cache: 'no-store' });
	return { response, body: await readBody(response) };
}

async function postLogin(secret) {
	const response = await fetch('/identity/login', {
		method: 'POST',
		credentials: 'include',
		cache: 'no-store',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ method: 'local.secret', name: 'admin', secret })
	});
	return { response, body: await readBody(response) };
}

async function expectAnonymousPrincipal(label) {
	const { response, body } = await getPrincipal();
	writeResponse(body);
	return addCheck(label, response.ok && body?.isAuthenticated === false);
}

async function expectAuthenticatedPrincipal() {
	const { response, body } = await getPrincipal();
	writeResponse(body);
	return addCheck('authenticated principal has admin role and account.manage permission',
		response.ok
		&& body?.isAuthenticated === true
		&& body?.roles?.includes('admin')
		&& body?.permissions?.includes('account.manage'));
}

async function expectAnonymousAdminRejected() {
	const { response, body } = await getAdminArea();
	writeResponse(body ?? `HTTP ${response.status}`);
	return addCheck('anonymous admin access is rejected with 401', response.status === 401);
}

async function expectAdminAllowed() {
	const { response, body } = await getAdminArea();
	writeResponse(body ?? `HTTP ${response.status}`);
	return addCheck('authorization policy allows admin area after login', response.ok && body?.requiredPermission === 'account.manage');
}

async function expectCorrectLogin() {
	const { response, body } = await postLogin('password');
	writeResponse(body);
	return addCheck('login succeeds and writes auth cookie', response.ok && body?.isAuthenticated === true);
}

async function expectWrongPasswordRejected() {
	const { response, body } = await postLogin('wrong');
	writeResponse(body ?? `HTTP ${response.status}`);
	return addCheck('wrong password is rejected', response.status === 401);
}

async function resetToAnonymous() {
	await postLogout();
}

async function runExclusive(action, clearChecks = false) {
	if (running)
		return;

	running = true;
	buttons.forEach(button => button.disabled = true);
	if (clearChecks)
		checks.replaceChildren();

	try {
		await action();
	} catch (error) {
		writeResponse(String(error));
		if (clearChecks)
			addCheck('sample request failed', false);
	} finally {
		running = false;
		buttons.forEach(button => button.disabled = false);
	}
}

async function showManualResult(label, action) {
	const { response, body } = await action();
	writeResponse({ action: label, status: response.status, body });
}

document.getElementById('all').addEventListener('click', () => runExclusive(async () => {
	// Run all checks owns the complete stateful sequence. No helper check secretly logs in or out.
	await resetToAnonymous();
	await expectAnonymousPrincipal('initial principal is anonymous');
	await expectAnonymousAdminRejected();
	await expectCorrectLogin();
	await expectAuthenticatedPrincipal();
	await expectAdminAllowed();
	await expectWrongPasswordRejected();
	await resetToAnonymous();
	await expectAnonymousPrincipal('logout returns to anonymous principal');
}, true));

document.getElementById('manual-login').addEventListener('click', () => runExclusive(async () => {
	await showManualResult('login as admin', () => postLogin('password'));
}, false));

document.getElementById('manual-logout').addEventListener('click', () => runExclusive(async () => {
	await showManualResult('logout', postLogout);
}, false));

document.getElementById('manual-principal').addEventListener('click', () => runExclusive(async () => {
	await showManualResult('show current principal', getPrincipal);
}, false));

document.getElementById('manual-admin').addEventListener('click', () => runExclusive(async () => {
	await showManualResult('try admin area', getAdminArea);
}, false));

document.getElementById('manual-wrong').addEventListener('click', () => runExclusive(async () => {
	await showManualResult('try wrong password', () => postLogin('wrong'));
}, false));
