# Babbly API Gateway - Load Testing Suite

This directory contains a comprehensive k6 load testing suite for the Babbly API Gateway deployed on Azure Kubernetes Service (AKS). The test suite follows the [Grafana load testing best practices](https://grafana.com/load-testing/types-of-load-testing/) and includes 6 different test categories.

## 📋 Test Categories

### 1. Smoke Test (`01-smoke-test.js`)
- **Purpose**: Verify basic functionality with minimal load
- **Duration**: ~4 minutes
- **Load**: 1 virtual user
- **Use Case**: Quick validation after deployments

### 2. Average Load Test (`02-average-load-test.js`)
- **Purpose**: Simulate normal expected traffic patterns
- **Duration**: ~19 minutes
- **Load**: 10-20 virtual users
- **Use Case**: Baseline performance validation

### 3. Stress Test (`03-stress-test.js`)
- **Purpose**: Test system under higher than normal load
- **Duration**: ~38 minutes
- **Load**: 50-100 virtual users
- **Use Case**: Verify stability under stress conditions

### 4. Spike Test (`04-spike-test.js`)
- **Purpose**: Test sudden traffic surges and auto-scaling
- **Duration**: ~16 minutes
- **Load**: 10-200 virtual users with rapid changes
- **Use Case**: Validate resilience to viral content scenarios

### 5. Breakpoint Test (`05-breakpoint-test.js`)
- **Purpose**: Find maximum system capacity and failure modes
- **Duration**: ~48 minutes
- **Load**: 25-1000 virtual users (gradual increase)
- **Use Case**: Capacity planning and bottleneck identification

### 6. Soak Test (`06-soak-test.js`)
- **Purpose**: Detect memory leaks and long-term stability issues
- **Duration**: ~7 hours
- **Load**: 30-50 virtual users (sustained)
- **Use Case**: Production readiness validation

## 🚀 Prerequisites

### 1. Install k6
```bash
# Windows (using Chocolatey)
choco install k6

# macOS (using Homebrew)
brew install k6

# Linux (Ubuntu/Debian)
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

### 2. Get Your AKS LoadBalancer IP
```bash
# Get the external IP of your API Gateway service
kubectl get service api-gateway -o wide

# Or use this command to get just the IP
kubectl get service api-gateway -o jsonpath='{.status.loadBalancer.ingress[0].ip}'
```

### 3. Update Test Configuration
Replace `YOUR_AKS_LOADBALANCER_IP` in all test files with your actual LoadBalancer IP:

```bash
# Example: If your LoadBalancer IP is 20.123.45.67
sed -i 's/YOUR_AKS_LOADBALANCER_IP/20.123.45.67/g' *.js
```

## 📊 Running the Tests

### Quick Start (Recommended Order)
```bash
# 1. Start with smoke test to verify everything works
k6 run 01-smoke-test.js

# 2. Run average load test for baseline
k6 run 02-average-load-test.js

# 3. Stress test to check limits
k6 run 03-stress-test.js

# 4. Spike test for resilience
k6 run 04-spike-test.js

# 5. Breakpoint test for capacity planning (resource intensive!)
k6 run 05-breakpoint-test.js

# 6. Soak test for long-term stability (7+ hours!)
k6 run 06-soak-test.js
```

### Advanced Options

#### Run with Custom Metrics Output
```bash
# Output metrics to InfluxDB
k6 run --out influxdb=http://localhost:8086/mydb 01-smoke-test.js

# Output to JSON file
k6 run --out json=results.json 01-smoke-test.js

# Output to CSV
k6 run --out csv=results.csv 01-smoke-test.js
```

#### Override Test Parameters
```bash
# Run with different user count
k6 run -e VUS=50 02-average-load-test.js

# Run with shorter duration
k6 run -e DURATION=5m 02-average-load-test.js

# Run with custom base URL
k6 run -e BASE_URL=http://your-custom-url:5010 01-smoke-test.js
```

## 📈 Interpreting Results

### Key Metrics to Monitor

1. **Response Time**
   - `http_req_duration`: Average response time
   - `p(95)`: 95th percentile response time
   - `p(99)`: 99th percentile response time

2. **Error Rates**
   - `http_req_failed`: Percentage of failed requests
   - `errors`: Custom error rate from test logic

3. **Throughput**
   - `http_reqs`: Total number of requests
   - `iterations`: Number of test iterations completed

4. **Custom Metrics**
   - `posts_created`: Number of posts successfully created
   - `feed_reads`: Number of feed requests
   - `concurrent_users`: Current number of active users

### Success Criteria

#### Smoke Test ✅
- All checks pass (100% success rate)
- Response times < 500ms
- No errors

#### Average Load Test ✅
- Error rate < 5%
- 95% of requests < 1000ms
- 99% of requests < 2000ms

#### Stress Test ✅
- Error rate < 15% (higher tolerance)
- 95% of requests < 2000ms
- System recovers after stress period

#### Spike Test ✅
- Error rate < 20% during spikes
- System auto-scales appropriately
- Recovery time < 2 minutes

#### Breakpoint Test ✅
- Identifies maximum sustainable load
- Clear failure patterns emerge
- Provides capacity planning data

#### Soak Test ✅
- No memory leaks (stable response times)
- Error rate < 5% throughout
- Resource usage remains stable

## 🔧 Troubleshooting

### Common Issues

1. **Connection Refused**
   ```
   Error: "dial: i/o timeout"
   ```
   - Verify LoadBalancer IP is correct
   - Check that port 5010 is accessible
   - Ensure AKS cluster is running

2. **High Error Rates**
   ```
   Error rate > threshold
   ```
   - Check pod resource limits
   - Verify database connections
   - Monitor Kubernetes events

3. **Slow Response Times**
   ```
   Response times > expected
   ```
   - Check pod CPU/memory usage
   - Verify network connectivity
   - Review database performance

### Monitoring During Tests

```bash
# Monitor pod status
kubectl get pods -w

# Check pod resource usage
kubectl top pods

# View pod logs
kubectl logs -f deployment/api-gateway

# Monitor services
kubectl get services

# Check events
kubectl get events --sort-by=.metadata.creationTimestamp
```

## 📋 Test Results Analysis

### Performance Baselines
Create baselines from your initial test runs:

```bash
# Example baseline metrics
Average Load Test Results:
- Response Time (avg): 450ms
- Response Time (p95): 800ms
- Error Rate: 2.1%
- Throughput: 45 req/s
```

### Capacity Planning
Use breakpoint test results for scaling decisions:

```bash
# Example capacity planning
Maximum Sustainable Load: 350 users
Recommended Operating Load: 245 users (70% of max)
Safe Operating Capacity: 175 users (50% of max)
```

## 🚨 Alerts and Monitoring

### Recommended Alerts
Set up these alerts based on your test results:

1. **Response Time**: > 95th percentile from average load test
2. **Error Rate**: > 5% for normal operations
3. **CPU Usage**: > 80% sustained
4. **Memory Usage**: > 85% sustained
5. **Pod Restart Count**: > 3 restarts in 5 minutes

### Integration with Monitoring
These tests work well with:
- **Prometheus**: For metrics collection
- **Grafana**: For visualization
- **Azure Monitor**: For AKS-specific metrics
- **Application Insights**: For application performance

## 📝 Best Practices

1. **Test Environment**: Use a staging environment that mirrors production
2. **Test Data**: Use realistic test data volumes
3. **Test Sequence**: Always start with smoke tests
4. **Resource Monitoring**: Monitor cluster resources during tests
5. **Gradual Scaling**: Don't jump directly to stress tests
6. **Documentation**: Document test results and system changes
7. **Regular Testing**: Run tests after every significant change

## 🔄 CI/CD Integration

### GitHub Actions Example
```yaml
name: Load Test
on:
  schedule:
    - cron: '0 2 * * *'  # Daily at 2 AM
  workflow_dispatch:

jobs:
  load-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Install k6
        run: |
          sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
          echo "deb https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
          sudo apt-get update
          sudo apt-get install k6
      - name: Run Smoke Test
        run: k6 run k8s/load-tests/01-smoke-test.js
```

## 📞 Support

For issues with the load tests:
1. Check this README
2. Review k6 documentation: https://k6.io/docs/
3. Check Grafana load testing guide: https://grafana.com/load-testing/
4. Monitor your AKS cluster health

---

**Remember**: Load testing can impact system performance. Always coordinate with your team and run tests during appropriate times. 