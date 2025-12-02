# Comprehensive Testing Guide - Global Disaster Management System

## 🎯 Testing Strategy

Given the scale of this application, we'll use a structured approach covering all major functionality areas.

---

## 1️⃣ Authentication & Authorization Testing

### Test 1.1: User Registration
- [ ] Navigate to `/UserAuthentication/Register`
- [ ] Fill form with valid data (optional location fields)
- [ ] Verify successful registration
- [ ] Check email/phone validation
- [ ] Try duplicate email (should fail)
- [ ] Test with/without location data

**Expected Results:**
- ✅ New user created in database
- ✅ Redirected to login page
- ✅ Validation errors shown for invalid data

### Test 1.2: User Login
- [ ] Navigate to `/UserAuthentication/Login`
- [ ] Login with valid credentials
- [ ] Verify Register/Login links hidden after login
- [ ] Check user menu shows (Profile, Logout)
- [ ] Try invalid credentials (should fail)
- [ ] Test "Remember Me" checkbox

**Test Accounts:**
```
Admin: admin@disastermgt.com / Admin@123
Regular User: (create your own)
```

### Test 1.3: Admin Access
- [ ] Login as admin
- [ ] Verify "Admin Panel" link appears in navbar
- [ ] Access `/Admin/Index`
- [ ] Verify admin dashboard loads with modern design
- [ ] Check all admin links work
- [ ] Logout and verify admin link disappears

### Test 1.4: Role-Based Access Control
- [ ] Try accessing `/Admin/Index` as regular user (should redirect)
- [ ] Try accessing `/Dashboard/Index` as anonymous user (should redirect)
- [ ] Verify user menu shows correctly based on role

---

## 2️⃣ Navigation & Layout Testing

### Test 2.1: Main Navigation
- [ ] Click all main menu items (Home, Tips, Emergency Response, Contact)
- [ ] Test dropdown menus (Tips, Emergency Response)
- [ ] Verify all links navigate correctly
- [ ] Check mobile responsiveness (toggle menu)
- [ ] Test language switcher

### Test 2.2: Responsive Design
**Desktop (1920x1080):**
- [ ] Check navbar layout
- [ ] Verify cards display in correct grid (4 columns)
- [ ] Test footer layout

**Tablet (768x1024):**
- [ ] Hamburger menu appears
- [ ] Cards stack to 2 columns
- [ ] All content readable

**Mobile (375x667):**
- [ ] Navbar collapses properly
- [ ] Cards stack to 1 column
- [ ] No horizontal scrolling
- [ ] Touch targets large enough

### Test 2.3: Language Switcher
- [ ] Click language dropdown
- [ ] Select different language
- [ ] Verify page content updates (if translations exist)
- [ ] Check cookie persists across pages
- [ ] Test with all available languages

---

## 3️⃣ Dashboard & Data Visualization Testing

### Test 3.1: User Dashboard
- [ ] Login and navigate to `/Dashboard/Index`
- [ ] Verify statistics cards display correctly
- [ ] Check charts render (if data exists)
- [ ] Test date range filters
- [ ] Verify data updates in real-time (if using SignalR)

### Test 3.2: Admin Dashboard
- [ ] Login as admin
- [ ] Navigate to `/Admin/Index`
- [ ] Verify all 4 management cards display
- [ ] Check hover effects work
- [ ] Click each card link and verify navigation
- [ ] Test quick stats cards at bottom

### Test 3.3: Analytics Dashboard
- [ ] Navigate to `/Analytics/Dashboard`
- [ ] Verify charts display with data
- [ ] Test filter functionality
- [ ] Check export functionality (if exists)
- [ ] Verify responsive layout

---

## 4️⃣ Incident Management Testing

### Test 4.1: View Incidents
- [ ] Navigate to `/Incident/Index`
- [ ] Verify incident list displays
- [ ] Check search/filter functionality
- [ ] Test pagination (if exists)
- [ ] Verify incident details page

### Test 4.2: Report Incident
- [ ] Navigate to `/Incident/Report`
- [ ] Fill incident report form
- [ ] Upload photos (if supported)
- [ ] Submit form
- [ ] Verify incident appears in list
- [ ] Check validation for required fields

### Test 4.3: Incident Map
- [ ] Navigate to `/Incident/Map`
- [ ] Verify map loads correctly
- [ ] Check incident markers display
- [ ] Click markers for details
- [ ] Test map zoom/pan functionality

---

## 5️⃣ Emergency Shelter Testing

### Test 5.1: Shelter List
- [ ] Navigate to `/Shelter/Index`
- [ ] Verify shelter list displays
- [ ] Check search functionality
- [ ] Test filter by location
- [ ] View shelter details

### Test 5.2: Find Nearby Shelter
- [ ] Navigate to `/Shelter/FindNearby`
- [ ] Allow location access
- [ ] Verify nearest shelters shown
- [ ] Check distance calculations
- [ ] Test "Get Directions" functionality

### Test 5.3: Shelter Map
- [ ] Navigate to `/Shelter/Map`
- [ ] Verify map displays all shelters
- [ ] Check marker clustering (if many shelters)
- [ ] Click markers for details
- [ ] Test filter by capacity/type

---

## 6️⃣ IoT Monitoring Testing

### Test 6.1: IoT Dashboard
- [ ] Navigate to `/IoTMonitoring/Index`
- [ ] Verify sensor list displays
- [ ] Check real-time data updates (SignalR)
- [ ] Test alert thresholds
- [ ] Verify sensor status indicators

### Test 6.2: Sensor Details
- [ ] Click on individual sensor
- [ ] View historical data
- [ ] Check charts for sensor readings
- [ ] Test date range filters
- [ ] Verify data export functionality

---

## 7️⃣ Location Management Testing (Admin Only)

### Test 7.1: Region/LGA Management
- [ ] Navigate to `/LGA/Index`
- [ ] View existing regions
- [ ] Create new region (`/LGA/CreateLGA`)
- [ ] Edit existing region
- [ ] Delete region (check constraints)
- [ ] Verify validation works

### Test 7.2: City Management
- [ ] Navigate to `/City/Index`
- [ ] View existing cities
- [ ] Create new city (`/City/CreateCity`)
- [ ] Assign city to region
- [ ] Edit city details
- [ ] Delete city (check constraints)

### Test 7.3: CSV Upload
- [ ] Navigate to `/CsvFile/Upload`
- [ ] Upload training data CSV
- [ ] Verify file validation
- [ ] Check data import success
- [ ] Test `/CsvFileCity/Upload` for predictions
- [ ] Delete uploaded files

---

## 8️⃣ User Management Testing (Admin Only)

### Test 8.1: User List
- [ ] Navigate to `/User/Index`
- [ ] Verify all users display
- [ ] Check search functionality
- [ ] Test filter by role
- [ ] View user details

### Test 8.2: User Actions
- [ ] Edit user details
- [ ] Change user role
- [ ] Deactivate/Activate user
- [ ] Delete user (if supported)
- [ ] Verify audit trail (if exists)

---

## 9️⃣ Tips & Information Testing

### Test 9.1: What To Do Page
- [ ] Navigate to `/Tips/WhatToDo`
- [ ] Verify content displays correctly
- [ ] Check responsive layout
- [ ] Test embedded media (if any)

### Test 9.2: Planning Ahead
- [ ] Navigate to `/Tips/Planning`
- [ ] Verify emergency planning tips
- [ ] Check downloadable resources (if any)

### Test 9.3: Report Disaster
- [ ] Navigate to `/Tips/ReportDisaster`
- [ ] Verify reporting options display
- [ ] Test links to reporting channels

### Test 9.4: Disaster Risks
- [ ] Navigate to `/Tips/DisasterRisks`
- [ ] Verify risk information displays
- [ ] Check charts/visualizations (if any)

---

## 🔟 Real-Time Features Testing (SignalR)

### Test 10.1: Disaster Alerts
- [ ] Login as user
- [ ] Trigger alert from admin panel (if supported)
- [ ] Verify toast notification appears
- [ ] Check alert sound plays
- [ ] Test multiple alerts

### Test 10.2: IoT Sensor Updates
- [ ] Open IoT Monitoring page
- [ ] Keep page open for 1-2 minutes
- [ ] Verify sensor readings update automatically
- [ ] Check connection status indicator

---

## 1️⃣1️⃣ Contact Form Testing

### Test 11.1: Submit Contact Form
- [ ] Navigate to `/ContactForms/ContactUs`
- [ ] Fill all required fields
- [ ] Submit form
- [ ] Verify success message
- [ ] Check validation for invalid data

### Test 11.2: Admin View Contact Forms
- [ ] Login as admin
- [ ] Navigate to contact forms list (if exists)
- [ ] Verify submitted forms display
- [ ] Test marking as read/responded

---

## 1️⃣2️⃣ SMS & Communication Testing

### Test 12.1: SMS Notifications
- [ ] Register with valid phone number
- [ ] Trigger SMS notification event
- [ ] Verify SMS sent (check logs)
- [ ] Test template rendering

### Test 12.2: USSD Integration
- [ ] Navigate to `/Ussd/Index` (if exists)
- [ ] Test USSD session handling
- [ ] Verify menu navigation
- [ ] Check data retrieval

### Test 12.3: WhatsApp Integration
- [ ] Test WhatsApp webhook endpoint
- [ ] Send message to WhatsApp bot
- [ ] Verify response received
- [ ] Check message history

### Test 12.4: Voice Calls
- [ ] Test voice webhook endpoint
- [ ] Trigger voice call notification
- [ ] Verify call connects
- [ ] Check call logs

---

## 1️⃣3️⃣ Predictions & Forecasting Testing

### Test 13.1: View Predictions
- [ ] Navigate to `/FloodData/Predictions`
- [ ] Verify predictions display
- [ ] Check date range filters
- [ ] Test location-based filtering
- [ ] Verify prediction accuracy indicators

### Test 13.2: ML Model Testing
- [ ] Upload training data
- [ ] Trigger model training (if manual)
- [ ] Verify model generates predictions
- [ ] Check prediction confidence scores

---

## 1️⃣4️⃣ Performance Testing

### Test 14.1: Page Load Times
- [ ] Measure home page load time (< 3 seconds)
- [ ] Check dashboard with large datasets
- [ ] Test map rendering with many markers
- [ ] Verify image optimization

### Test 14.2: Concurrent Users
- [ ] Simulate 10+ concurrent logins
- [ ] Check server response times
- [ ] Verify SignalR handles multiple connections
- [ ] Test database query performance

---

## 1️⃣5️⃣ Security Testing

### Test 15.1: XSS Prevention
- [ ] Try entering `<script>alert('XSS')</script>` in forms
- [ ] Verify input sanitization works
- [ ] Check URL parameter encoding

### Test 15.2: SQL Injection
- [ ] Try SQL injection in search fields
- [ ] Test with `' OR '1'='1`
- [ ] Verify parameterized queries used

### Test 15.3: CSRF Protection
- [ ] Verify anti-forgery tokens in forms
- [ ] Test POST requests without tokens
- [ ] Check token validation

### Test 15.4: Authorization Bypass
- [ ] Try accessing admin URLs as user
- [ ] Test API endpoints without auth
- [ ] Verify role enforcement

---

## 1️⃣6️⃣ Browser Compatibility Testing

Test on:
- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Edge (latest)
- [ ] Safari (macOS/iOS)
- [ ] Mobile browsers (Chrome Mobile, Safari iOS)

Check:
- Layout consistency
- JavaScript functionality
- CSS rendering
- Form submissions
- SignalR connections

---

## 1️⃣7️⃣ Database Testing

### Test 17.1: Data Integrity
- [ ] Create records with foreign keys
- [ ] Try deleting referenced records
- [ ] Verify cascade delete behavior
- [ ] Check constraint enforcement

### Test 17.2: Nullable Fields
- [ ] Register user without location data
- [ ] Verify NULL values accepted
- [ ] Check queries handle NULL correctly

---

## 1️⃣8️⃣ Error Handling Testing

### Test 18.1: 404 Pages
- [ ] Navigate to non-existent URL
- [ ] Verify custom 404 page (if configured)
- [ ] Check navigation back to site

### Test 18.2: 500 Errors
- [ ] Trigger server error (if test endpoint exists)
- [ ] Verify error page displays
- [ ] Check error logging

### Test 18.3: Validation Errors
- [ ] Submit forms with missing required fields
- [ ] Verify validation messages display
- [ ] Check field-level error highlighting

---

## 🧪 Automated Testing Approach

### Unit Tests (Recommended)
```bash
cd FloodManagementSystem.Tests
dotnet test
```

**Test Coverage:**
- Repository methods
- Service layer logic
- Controller actions
- Data validation
- Business rules

### Integration Tests
- API endpoint testing
- Database operations
- SignalR hub testing
- External service mocking

---

## 📋 Testing Checklist Summary

### Critical Priority (Must Test)
- [x] User registration & login
- [x] Admin access & dashboard
- [x] Navigation menu (all links)
- [x] Responsive design (mobile/tablet/desktop)
- [x] Language switcher
- [ ] Incident reporting
- [ ] Real-time alerts (SignalR)
- [ ] Location management (admin)
- [ ] User management (admin)

### High Priority
- [ ] Dashboard data visualization
- [ ] Shelter management
- [ ] IoT monitoring
- [ ] Predictions display
- [ ] Contact form submission
- [ ] CSV upload functionality

### Medium Priority
- [ ] Tips pages content
- [ ] Analytics charts
- [ ] SMS notifications
- [ ] WhatsApp integration
- [ ] USSD functionality
- [ ] Voice calls

### Low Priority (Nice to Have)
- [ ] Performance optimization
- [ ] Browser compatibility edge cases
- [ ] Advanced security testing
- [ ] Load testing

---

## 🐛 Bug Tracking Template

When you find issues, document them:

```
**Bug Title:** [Concise description]

**Severity:** Critical / High / Medium / Low

**Steps to Reproduce:**
1. Navigate to...
2. Click on...
3. Enter...

**Expected Result:**
What should happen

**Actual Result:**
What actually happens

**Environment:**
- Browser: 
- Screen size:
- User role:

**Screenshots:**
[Attach if applicable]
```

---

## ✅ Testing Completion Criteria

Your application is production-ready when:

- [ ] All critical priority tests pass
- [ ] No blocking bugs remain
- [ ] Responsive design works on all devices
- [ ] Authentication/authorization solid
- [ ] Admin functions work correctly
- [ ] Real-time features functional
- [ ] Performance acceptable (< 3s page loads)
- [ ] No console errors in browser
- [ ] Database operations reliable
- [ ] Error handling graceful

---

## 🚀 Quick Test Commands

```powershell
# Build and check for errors
dotnet build

# Run application
dotnet run

# Run unit tests
cd FloodManagementSystem.Tests
dotnet test --verbosity normal

# Check database migrations
dotnet ef migrations list

# View application logs
# Check console output while running
```

---

## 📞 Need Help?

If you encounter issues during testing:
1. Check browser console for JavaScript errors
2. Review application logs (console output)
3. Verify database state with SQL queries
4. Test with different user roles
5. Clear browser cache and try again

---

**Happy Testing! 🎉**
