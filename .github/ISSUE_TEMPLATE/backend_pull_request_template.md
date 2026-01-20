### feat:
<!-- Example: Implement caregiver registration vertical slice -->

### Description
<!--Example: Adds complete caregiver registration functionality us to... -->

### Changes Made
<!--
Backend (HealthCareAB_v1)
AuthService: Added RegisterCaregiverAsync() method to handle caregiver registration with:

Email validation and duplicate prevention
Password hashing via UserManager
ApplicationUser creation with "Caregiver" role assignment
Caregiver entity creation and persistence
Transaction safety with rollback on failure
AuthController: Added POST /api/auth/register-caregiver endpoint -->

### Tests (HealthCareAB.Test)

<!-- Example:
Added comprehensive test suite CaregiverRegistrationTests covering:

✅ Successful registration with valid data
✅ Duplicate email prevention
✅ Password hashing verification
✅ Weak password rejection
✅ Correct "Caregiver" role assignment
✅ Caregiver entity data integrity
✅ Transaction rollback on failure
Test Results: 7/7 passing
-->

### Architecture Notes
<!-- Example: Uses vertical slice pattern: Data layer → Service layer → API endpoint -->
<!-- Example: Transaction safety: All changes rolled back if any step fails -->


### Related Issues
<!-- Example: Implements issue #96: Register Caregiver -->


### Testing
All tests passing:

<!-- Example: 7 caregiver registration tests
Existing patient registration tests fixed and passing
Total: 105/105 tests passing -->

### Checklist
- [ ] Code follows project conventions
- [ ] Tests added and passing
- [ ] Error handling implemented
- [ ] Transaction safety ensured
- [ ] DTOs with validation
