#include <windows.h>
#include <chai3d/chai3d.h>

#define FALCON_API __declspec( dllexport )

chai3d::cHapticDeviceHandler* handler;
std::map<unsigned int, chai3d::cGenericHapticDevicePtr> open_devices;

extern "C" {

typedef struct {
    double x, y, z;
} vector;

FALCON_API bool connect_haptic(const unsigned int index) {
    chai3d::cGenericHapticDevicePtr haptic_device;
    const auto device = handler->getDevice(haptic_device, index);
    if (device) {
        open_devices[index] = haptic_device;
    }
    return device;
}

FALCON_API bool close_haptic(const unsigned int index) {
    const auto device = open_devices[index];
    if (device == nullptr) return false;
    open_devices.erase(index);
    device->close();
    return true;
}

FALCON_API vector get_position(const unsigned int index) {
    const auto device = open_devices[index];
    if (device == nullptr) return {0, 0, 0};
    chai3d::cVector3d c_vector;
    device->getPosition(c_vector);
    vector v;
    v.x = c_vector.x();
    v.y = c_vector.y();
    v.z = c_vector.z();
    return v;
}

FALCON_API void apply_force(const unsigned int index, const vector force) {
    const auto device = open_devices[index];
    if (device == nullptr) return;
    device->setForce({force.x, force.y, force.z});
}

}


BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH:
        handler = new chai3d::cHapticDeviceHandler();
        break;
    case DLL_PROCESS_DETACH:
        delete handler;
    default:
        break;
    }

    return TRUE;
}
