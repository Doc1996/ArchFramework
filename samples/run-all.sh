#!/usr/bin/env bash
set -e

pids=()

start_sample() {
	dotnet run --project "$1" --urls "$2" &
	pids+=("$!")
}

cleanup() {
	for pid in "${pids[@]}"; do
		kill "$pid" 2>/dev/null || true
	done
}

trap cleanup SIGINT SIGTERM EXIT

start_sample samples/Leva.Framework.Sample.StateCounter http://localhost:5101
start_sample samples/Leva.Framework.Sample.StorageDesk http://localhost:5102
start_sample samples/Leva.Framework.Sample.LiveDashboard http://localhost:5103
start_sample samples/Leva.Framework.Sample.LocalIdentity http://localhost:5104
start_sample samples/Leva.Framework.Sample.WebIdentity http://localhost:5105

echo "StateCounter:  http://localhost:5101"
echo "StorageDesk:   http://localhost:5102"
echo "LiveDashboard: http://localhost:5103"
echo "LocalIdentity: http://localhost:5104"
echo "WebIdentity:   http://localhost:5105"
echo
echo "Press Ctrl+C to stop all samples."

wait
