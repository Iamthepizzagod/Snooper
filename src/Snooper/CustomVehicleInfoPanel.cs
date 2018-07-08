﻿// <copyright file="CustomVehicleInfoPanel.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace Snooper
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A customized vehicle info panel that additionally shows the origin building of the owner citizen.
    /// </summary>
    internal sealed class CustomVehicleInfoPanel : CustomInfoPanelBase<VehicleWorldInfoPanel>
    {
        private const string GameInfoPanelName = "(Library) CitizenVehicleWorldInfoPanel";
        private GetDriverInstanceDelegate getDriverInstance;

        private CustomVehicleInfoPanel(string panelName)
            : base(panelName)
        {
            try
            {
                getDriverInstance = FastDelegate.Create<PassengerCarAI, GetDriverInstanceDelegate>("GetDriverInstance");
            }
            catch (Exception ex)
            {
                Debug.LogError($"The 'Snooper' mod failed to obtain the GetDriverInstance method. Error message: " + ex);
            }
        }

        private delegate ushort GetDriverInstanceDelegate(PassengerCarAI instance, ushort vehicleId, ref Vehicle vehicle);

        /// <summary>Enables the vehicle info panel customization. Can return null on failure.</summary>
        /// <returns>An instance of the <see cref="CustomVehicleInfoPanel"/> object that can be used for disabling
        /// the customization, or null when the customization fails.</returns>
        public static CustomVehicleInfoPanel Enable()
        {
            var result = new CustomVehicleInfoPanel(GameInfoPanelName);
            return result.IsValid ? result : null;
        }

        /// <summary>Updates the origin building display.</summary>
        /// <param name="instance">The game object instance to get the information from.</param>
        public override void UpdateOrigin(ref InstanceID instance)
        {
            ushort instanceId = 0;
            try
            {
                if (getDriverInstance == null)
                {
                    Debug.Log("SNOOPER: delegate is null");
                    return;
                }

                if (instance.Type != InstanceType.Vehicle || instance.Vehicle == 0)
                {
                    Debug.Log($"SNOOPER: not vehicle or vehicle instance == 0");
                    return;
                }

                ushort vehicleId = instance.Vehicle;
                vehicleId = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].GetFirstVehicle(vehicleId);
                if (vehicleId == 0)
                {
                    Debug.Log("SNOOPER: first vehicle is null");
                    return;
                }

                VehicleInfo vehicleInfo = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].Info;
                if (vehicleInfo.m_vehicleType != VehicleInfo.VehicleType.Bicycle && !(vehicleInfo.m_vehicleAI is PassengerCarAI))
                {
                    Debug.Log($"SNOOPER: vehicle type wrong: {vehicleInfo.m_vehicleType}, type = {vehicleInfo.m_vehicleAI.GetType().Name}");
                    return;
                }

                try
                {
                    // Same implementation for Bicycle and Passenger Car.
                    // HACK: calling an instance method with NULL as 'this', but luckily 'this' is not used there.
                    instanceId = getDriverInstance(null, vehicleId, ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId]);
                    Debug.Log($"SNOOPER: instanceId = {instanceId}");
                }
                catch (Exception ex)
                {
                    Debug.Log($"SNOOPER: exception on calling delegate: " + ex);
                    getDriverInstance = null;
                }
            }
            finally
            {
                UpdateOrigin(instanceId);
            }
        }
    }
}