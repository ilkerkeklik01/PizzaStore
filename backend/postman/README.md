# PizzaStore E2E Automation Tests

This folder contains comprehensive end-to-end automation tests for the PizzaStore API using Postman.

## 📦 Files

- **PizzaStore-E2E-Tests.postman_collection.json** - Main test collection with automated test scripts
- **PizzaStore.postman_environment.json** - Environment configuration with variables
- **openapi-spec.json** - OpenAPI specification from Swagger

## 🚀 Quick Start

### 1. Import Collection & Environment

1. Open Postman
2. Click **Import** button
3. Import both files:
   - `PizzaStore-E2E-Tests.postman_collection.json`
   - `PizzaStore.postman_environment.json`
4. Select "PizzaStore Environment" from the environment dropdown

### 2. Configure Environment

Update the `base_url` variable if your API runs on a different address:
- Default: `http://localhost:5000`
- Update in environment settings if needed

### 3. Start Your API

Make sure your PizzaStore API is running:
```bash
dotnet run --project src/PizzaStore.Api
```

**Authentication Note:** All authenticated requests use `Authorization: Bearer {token}` header format. The collection handles this automatically by storing and including the JWT token from login responses.

### 4. Run Tests

#### Option A: Run Entire Collection
1. Click on "PizzaStore E2E Tests" collection
2. Click **Run** button (or press ⌘/Ctrl + R)
3. Click **Run PizzaStore E2E Tests**
4. Watch all tests execute automatically

#### Option B: Run Individual Folders
- Right-click any folder (e.g., "Shopping Cart Flow")
- Select **Run folder**

#### Option C: Run Single Request
- Click any request
- Click **Send** button
- View test results in the "Test Results" tab

## 📋 Test Coverage

### 1. Setup & Authentication (3 tests)
- ✅ Register new user with unique email
- ✅ Login and receive JWT token
- ✅ Get authenticated user details

### 2. Browse Menu Flow (3 tests)
- ✅ Get all available pizzas
- ✅ Get specific pizza details with variants
- ✅ Get available toppings

### 3. Shopping Cart Flow (6 tests)
- ✅ Get empty cart for new user
- ✅ Add pizza to cart with toppings
- ✅ Increase item quantity
- ✅ Decrease item quantity
- ✅ View updated cart
- ✅ Verify cart total calculations

### 4. Order Placement Flow (4 tests)
- ✅ Checkout and create order from cart
- ✅ Get user's order history
- ✅ Get specific order details
- ✅ Cancel order

### 5. Admin Flow (3 tests - Optional)
- ✅ Admin login with credentials
- ✅ Get all users (admin only)
- ✅ Get all orders (admin only)

### 6. Negative Test Cases (4 tests)
- ✅ Reject unauthenticated requests
- ✅ Reject invalid login credentials
- ✅ Reject invalid cart items
- ✅ Reject user access to admin endpoints

## 🧪 Test Automation Features

### Automatic Variable Management
The collection automatically:
- Generates unique test user emails with timestamps
- Stores authentication tokens
- Saves entity IDs for subsequent requests
- Chains requests together seamlessly

### Comprehensive Assertions
Each request includes multiple test assertions:
- HTTP status code validation
- Response structure verification
- Data integrity checks
- Business logic validation

### Pre-request Scripts
- Generate dynamic test data
- Set up required variables
- Prepare request payloads

### Test Scripts
- Validate responses
- Extract and store data
- Chain requests together
- Verify business rules

## 📊 Running Tests via CLI

You can also run tests using Newman (Postman CLI):

### Install Newman
```bash
npm install -g newman
```

### Run Collection
```bash
newman run PizzaStore-E2E-Tests.postman_collection.json \
  -e PizzaStore.postman_environment.json \
  --reporters cli,json,html \
  --reporter-html-export results.html
```

### Run with Custom Base URL
```bash
newman run PizzaStore-E2E-Tests.postman_collection.json \
  -e PizzaStore.postman_environment.json \
  --env-var "base_url=https://api.yourserver.com"
```

## 🔧 Environment Variables

| Variable | Description | Auto-populated |
|----------|-------------|----------------|
| `base_url` | API base URL | ❌ (Manual) |
| `auth_token` | User JWT token | ✅ |
| `admin_token` | Admin JWT token | ✅ |
| `test_email` | Generated test email | ✅ |
| `test_password` | Test user password | ✅ |
| `userId` | Registered user ID | ✅ |
| `pizza_id` | Sample pizza ID | ✅ |
| `variant_id` | Pizza variant ID | ✅ |
| `topping_id_1` | First topping ID | ✅ |
| `topping_id_2` | Second topping ID | ✅ |
| `cart_item_id` | Cart item ID | ✅ |
| `order_id` | Created order ID | ✅ |

## 🎯 Test Execution Order

**Important:** Run tests in sequence for E2E flows:

1. **Setup & Authentication** - Must run first
2. **Browse Menu Flow** - Discovers available products
3. **Shopping Cart Flow** - Builds cart for checkout
4. **Order Placement Flow** - Completes purchase
5. **Admin Flow** - Optional, can run independently
6. **Negative Test Cases** - Can run anytime

## ✅ Expected Results

All tests should pass when:
- API is running and accessible
- Database is seeded with initial data
- Default admin credentials are available
- All endpoints are functioning correctly

## 🐛 Troubleshooting

### Tests Fail with 401 Unauthorized
- Ensure authentication flow runs first
- Check if token is being stored in environment
- Verify JWT is not expired

### Tests Fail with 404 Not Found
- Confirm API is running on correct port
- Check `base_url` environment variable
- Verify API routes match OpenAPI spec

### Tests Fail with 500 Internal Server Error
- Check API logs for detailed error
- Ensure database is accessible
- Verify data seeding completed successfully

### Admin Tests Fail with 403 Forbidden
- Confirm default admin account exists
- Check admin credentials: `admin@pizzastore.com` / `Admin123!`

## 📝 Customization

### Add New Tests
1. Duplicate existing request
2. Update request details
3. Add test scripts using Postman's test snippets
4. Save to appropriate folder

### Modify Test Data
Edit pre-request scripts to customize:
- User registration details
- Order delivery addresses
- Product quantities
- Test scenarios

### Add Test Reports
Use Newman reporters for various outputs:
- HTML: `--reporters html`
- JSON: `--reporters json`
- JUnit: `--reporters junit`

## 🔗 Integration with CI/CD

Example GitHub Actions workflow:

```yaml
- name: Run API Tests
  run: |
    npm install -g newman
    newman run postman/PizzaStore-E2E-Tests.postman_collection.json \
      -e postman/PizzaStore.postman_environment.json \
      --reporters cli,junit \
      --reporter-junit-export test-results.xml
```

## 📚 Resources

- [Postman Documentation](https://learning.postman.com/docs/)
- [Newman CLI](https://learning.postman.com/docs/running-collections/using-newman-cli/command-line-integration-with-newman/)
- [Writing Tests in Postman](https://learning.postman.com/docs/writing-scripts/test-scripts/)

## 📧 Support

For issues or questions:
- Email: ilkerkeklik50@gmail.com
- Check API logs for detailed errors
- Review test assertions for failure reasons
