#!/bin/bash

# Babbly API Gateway Load Test Runner
# This script automates the process of running k6 load tests against your AKS deployment

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
NAMESPACE="default"
SERVICE_NAME="api-gateway"
RESULTS_DIR="./results"
TEST_DIR="."

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check prerequisites
check_prerequisites() {
    print_info "Checking prerequisites..."
    
    # Check if k6 is installed
    if ! command -v k6 &> /dev/null; then
        print_error "k6 is not installed. Please install k6 first."
        print_info "Installation instructions:"
        print_info "  macOS: brew install k6"
        print_info "  Windows: choco install k6"
        print_info "  Linux: Check README.md for instructions"
        exit 1
    fi
    
    # Check if kubectl is installed
    if ! command -v kubectl &> /dev/null; then
        print_error "kubectl is not installed. Please install kubectl first."
        exit 1
    fi
    
    print_success "Prerequisites check passed"
}

# Function to get LoadBalancer IP
get_loadbalancer_ip() {
    print_info "Getting LoadBalancer IP for service ${SERVICE_NAME}..."
    
    # Check if service exists
    if ! kubectl get service ${SERVICE_NAME} -n ${NAMESPACE} &> /dev/null; then
        print_error "Service ${SERVICE_NAME} not found in namespace ${NAMESPACE}"
        print_info "Available services:"
        kubectl get services -n ${NAMESPACE}
        exit 1
    fi
    
    # Get the external IP
    local ip=""
    local attempts=0
    local max_attempts=30
    
    while [ -z "$ip" ] && [ $attempts -lt $max_attempts ]; do
        ip=$(kubectl get service ${SERVICE_NAME} -n ${NAMESPACE} -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "")
        
        if [ -z "$ip" ]; then
            print_warning "LoadBalancer IP not yet available. Waiting... (attempt $((attempts + 1))/${max_attempts})"
            sleep 5
            ((attempts++))
        fi
    done
    
    if [ -z "$ip" ]; then
        print_error "Failed to get LoadBalancer IP after ${max_attempts} attempts"
        print_info "Service status:"
        kubectl get service ${SERVICE_NAME} -n ${NAMESPACE}
        exit 1
    fi
    
    print_success "LoadBalancer IP: ${ip}"
    echo $ip
}

# Function to update test files with correct IP
update_test_files() {
    local ip=$1
    print_info "Updating test files with LoadBalancer IP: ${ip}"
    
    # Create backup of original files if they exist
    for file in ${TEST_DIR}/*.js; do
        if [ -f "$file" ]; then
            if grep -q "YOUR_AKS_LOADBALANCER_IP" "$file"; then
                cp "$file" "$file.backup"
                sed -i.tmp "s/YOUR_AKS_LOADBALANCER_IP/${ip}/g" "$file"
                rm "$file.tmp" 2>/dev/null || true
            fi
        fi
    done
    
    print_success "Test files updated"
}

# Function to restore test files
restore_test_files() {
    print_info "Restoring original test files..."
    
    for file in ${TEST_DIR}/*.js.backup; do
        if [ -f "$file" ]; then
            original_file="${file%.backup}"
            mv "$file" "$original_file"
        fi
    done
    
    print_success "Original test files restored"
}

# Function to setup results directory
setup_results_dir() {
    mkdir -p ${RESULTS_DIR}
    local timestamp=$(date +"%Y%m%d_%H%M%S")
    RESULTS_DIR="${RESULTS_DIR}/load_test_${timestamp}"
    mkdir -p ${RESULTS_DIR}
    print_success "Results directory created: ${RESULTS_DIR}"
}

# Function to run a single test
run_test() {
    local test_file=$1
    local test_name=$(basename "$test_file" .js)
    
    print_info "Running ${test_name}..."
    
    local output_file="${RESULTS_DIR}/${test_name}_results.json"
    local log_file="${RESULTS_DIR}/${test_name}_log.txt"
    
    # Run the test with JSON output and capture logs
    if k6 run --out json="${output_file}" "${test_file}" 2>&1 | tee "${log_file}"; then
        print_success "${test_name} completed successfully"
        
        # Extract summary from log
        if grep -q "✓\|✗" "${log_file}"; then
            print_info "Test Summary for ${test_name}:"
            grep "✓\|✗\|http_req_duration\|http_req_failed" "${log_file}" | head -10
        fi
    else
        print_error "${test_name} failed"
        return 1
    fi
    
    echo ""
}

# Function to run health check
run_health_check() {
    local ip=$1
    print_info "Running health check against http://${ip}:5010/health"
    
    if curl -s --max-time 10 "http://${ip}:5010/health" > /dev/null; then
        print_success "Health check passed"
        return 0
    else
        print_error "Health check failed"
        return 1
    fi
}

# Function to show usage
show_usage() {
    echo "Usage: $0 [OPTIONS] [TEST_TYPE]"
    echo ""
    echo "Options:"
    echo "  -h, --help              Show this help message"
    echo "  -n, --namespace NAME    Kubernetes namespace (default: ${NAMESPACE})"
    echo "  -s, --service NAME      Service name (default: ${SERVICE_NAME})"
    echo "  -r, --results-dir DIR   Results directory (default: ${RESULTS_DIR})"
    echo "  --ip IP                 Use specific LoadBalancer IP"
    echo "  --no-health-check       Skip health check before tests"
    echo "  --keep-files           Don't restore original test files"
    echo ""
    echo "Test Types:"
    echo "  smoke                   Run smoke test only"
    echo "  average                 Run average load test only"
    echo "  stress                  Run stress test only"
    echo "  spike                   Run spike test only"
    echo "  breakpoint              Run breakpoint test only"
    echo "  soak                    Run soak test only"
    echo "  all                     Run all tests in sequence"
    echo "  suite                   Run recommended test suite (smoke + average + stress)"
    echo ""
    echo "Examples:"
    echo "  $0 smoke                # Run smoke test"
    echo "  $0 suite                # Run recommended test suite"
    echo "  $0 --ip 20.123.45.67 all  # Run all tests with specific IP"
}

# Main execution function
main() {
    local test_type=""
    local custom_ip=""
    local skip_health_check=false
    local keep_files=false
    
    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            -h|--help)
                show_usage
                exit 0
                ;;
            -n|--namespace)
                NAMESPACE="$2"
                shift 2
                ;;
            -s|--service)
                SERVICE_NAME="$2"
                shift 2
                ;;
            -r|--results-dir)
                RESULTS_DIR="$2"
                shift 2
                ;;
            --ip)
                custom_ip="$2"
                shift 2
                ;;
            --no-health-check)
                skip_health_check=true
                shift
                ;;
            --keep-files)
                keep_files=true
                shift
                ;;
            smoke|average|stress|spike|breakpoint|soak|all|suite)
                test_type="$1"
                shift
                ;;
            *)
                print_error "Unknown option: $1"
                show_usage
                exit 1
                ;;
        esac
    done
    
    # Default to smoke test if no test type specified
    if [ -z "$test_type" ]; then
        test_type="smoke"
        print_warning "No test type specified, defaulting to smoke test"
    fi
    
    print_info "Starting Babbly API Gateway Load Tests"
    print_info "Test type: ${test_type}"
    print_info "Namespace: ${NAMESPACE}"
    print_info "Service: ${SERVICE_NAME}"
    
    # Check prerequisites
    check_prerequisites
    
    # Get LoadBalancer IP
    if [ -n "$custom_ip" ]; then
        loadbalancer_ip="$custom_ip"
        print_info "Using custom IP: ${loadbalancer_ip}"
    else
        loadbalancer_ip=$(get_loadbalancer_ip)
    fi
    
    # Run health check
    if [ "$skip_health_check" = false ]; then
        if ! run_health_check "$loadbalancer_ip"; then
            print_error "Health check failed. Aborting tests."
            exit 1
        fi
    fi
    
    # Setup results directory
    setup_results_dir
    
    # Update test files with correct IP
    update_test_files "$loadbalancer_ip"
    
    # Trap to restore files on exit
    if [ "$keep_files" = false ]; then
        trap restore_test_files EXIT
    fi
    
    # Run tests based on type
    case $test_type in
        smoke)
            run_test "${TEST_DIR}/01-smoke-test.js"
            ;;
        average)
            run_test "${TEST_DIR}/02-average-load-test.js"
            ;;
        stress)
            run_test "${TEST_DIR}/03-stress-test.js"
            ;;
        spike)
            run_test "${TEST_DIR}/04-spike-test.js"
            ;;
        breakpoint)
            print_warning "Breakpoint test is resource intensive and may take ~48 minutes"
            read -p "Do you want to continue? (y/N) " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                run_test "${TEST_DIR}/05-breakpoint-test.js"
            else
                print_info "Breakpoint test skipped"
            fi
            ;;
        soak)
            print_warning "Soak test runs for ~7 hours"
            read -p "Do you want to continue? (y/N) " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                run_test "${TEST_DIR}/06-soak-test.js"
            else
                print_info "Soak test skipped"
            fi
            ;;
        suite)
            print_info "Running recommended test suite..."
            run_test "${TEST_DIR}/01-smoke-test.js"
            run_test "${TEST_DIR}/02-average-load-test.js"
            run_test "${TEST_DIR}/03-stress-test.js"
            ;;
        all)
            print_warning "Running all tests will take several hours"
            read -p "Do you want to continue? (y/N) " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                run_test "${TEST_DIR}/01-smoke-test.js"
                run_test "${TEST_DIR}/02-average-load-test.js"
                run_test "${TEST_DIR}/03-stress-test.js"
                run_test "${TEST_DIR}/04-spike-test.js"
                run_test "${TEST_DIR}/05-breakpoint-test.js"
                run_test "${TEST_DIR}/06-soak-test.js"
            else
                print_info "Full test suite skipped"
            fi
            ;;
    esac
    
    print_success "Load testing completed!"
    print_info "Results saved to: ${RESULTS_DIR}"
    print_info "To view results:"
    print_info "  ls -la ${RESULTS_DIR}"
    
    # Generate summary report
    cat > "${RESULTS_DIR}/summary.md" << EOF
# Load Test Summary

**Date**: $(date)
**LoadBalancer IP**: ${loadbalancer_ip}
**Test Type**: ${test_type}
**Namespace**: ${NAMESPACE}
**Service**: ${SERVICE_NAME}

## Files Generated
$(ls -la ${RESULTS_DIR} | grep -v "^total\|^d")

## Next Steps
1. Review the JSON results files for detailed metrics
2. Check log files for any errors or warnings
3. Compare results with your performance baselines
4. Update monitoring alerts based on test results

## Useful Commands
\`\`\`bash
# View test results
cat ${RESULTS_DIR}/*_results.json | jq

# Check for errors in logs
grep -i error ${RESULTS_DIR}/*_log.txt

# Monitor your AKS cluster
kubectl get pods -w
kubectl top pods
\`\`\`
EOF
    
    print_info "Summary report generated: ${RESULTS_DIR}/summary.md"
}

# Run main function with all arguments
main "$@" 