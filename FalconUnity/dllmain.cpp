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
        haptic_device->open();
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
    if (device == nullptr) return {NAN, NAN, NAN};
    chai3d::cVector3d c_vector;
    device->getPosition(c_vector);
    vector v;
    v.x = c_vector.x();
    v.y = c_vector.y();
    v.z = c_vector.z();
    return v;
}

FALCON_API bool is_device_connected(const unsigned int index) {
    return open_devices.find(index) != open_devices.cend();
}

FALCON_API bool apply_force(const unsigned int index, const vector force) {
    const auto device = open_devices[index];
    if (device == nullptr) return false;
    device->setForce({force.x, force.y, force.z});
    return true;
}

FALCON_API bool is_button_pressed(const unsigned int index, const int button_id) {
    const auto device = open_devices[index];
    if (device == nullptr) return false;
    bool status;
    device->getUserSwitch(button_id, status);
    return status;
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
